declare var $: any;
declare var Tribute: any;

var tribute = new Tribute({
    collection: [
        {
            trigger: '@',
            values: function (text: string, cb: any): void {
                if (text.trim().length > 0) {
                    $.ajax({
                        method: 'GET',
                        url: '/Person/Completions/' + text,
                        dataType: 'json'
                    }).then(r => r.items, () => []).then((items: any[]) => {
                        cb(items);
                    });
                } else {
                    cb([]);
                }
            },
            lookup: function (person: any, mentionText: string) {
                return person.label + '-' + person.value;
            },
            fillAttr: 'label',
            selectTemplate: function (item) {
                return '@' + item.original.label;
            },
            menuItemTemplate: function (item) {
                return item.original.display + ' <i>@' + item.original.label + '</i>';
            },
        },
        {
            trigger: '#',
            values: function (text: string, cb: any): void {
                if (text.trim().length > 0) {
                    $.ajax({
                        method: 'GET',
                        url: '/Tag/Completions/' + text,
                        dataType: 'json'
                    }).then(r => r.items, () => []).then((items: any[]) => {
                        cb(items);
                    });
                } else {
                    cb([]);
                }
            },
            lookup: function (person: any, mentionText: string) {
                return person.label + '-' + person.value;
            },
            fillAttr: 'label',
            selectTemplate: function (item) {
                return '#' + item.original.label;
            },
            menuItemTemplate: function (item) {
                return '#' + item.original.label;
            },
        }
    ]
});


var ItemEditorId: number = 0;
var CachedItemEditors: any = {};

class TagLikeItemEditorClasses {
    public editor = '';
    public search = '';
    public persist = '';
    public url = '';
    public deleteItem = '';
    public item = '';
};

class TagLikeItemEditorOptions {
    public classes = new TagLikeItemEditorClasses();
};

class ItemEditor {
    public id: number;
    public items: any[];
    public element: any;
    public searchFieldElement: HTMLInputElement;
    public saveElement: any;
    public classes = new TagLikeItemEditorClasses();

    public textareaEditor: any = undefined;
    public textcomplete: any = undefined;
    
    public completionUrl: string;
    public maxCount: number;

    constructor(element: any | string | undefined, options: TagLikeItemEditorOptions) {
        $.extend(this, options || new TagLikeItemEditorOptions());
        this.id = ItemEditorId;
        ItemEditorId++;
        CachedItemEditors[this.id] = this;

        this.items = [];
        let $elem = (typeof element === 'string' || element === undefined) ? $(element as string || this.classes.editor) : element;
        this.element = $elem;
        this.searchFieldElement = $('.' + this.classes.search, this.element)[0] as HTMLInputElement;
        this.saveElement = $('.' + this.classes.persist, this.element);

        $(this.element).attr('data-item-editor-id', this.id);
        this.maxCount = parseInt($(this.element).attr('data-max-count') || '99');
        
        if ($(this.element).is('textarea.js-item-editable')) {
            tribute.attach(this.element);
        } else {
            // @ts-ignore
            this.searchFieldElement.autocomplete({
                minLength: 0,
                source: this.source.bind(this),
                select: this.select.bind(this),
            });

            this.completionUrl = $(this.element).find('.' + this.classes.url).prop('href');
            if (typeof this.completionUrl !== 'string') {
                throw new Error('this.completionUrl is not a string (is ' + (typeof this.completionUrl) + ')');
            }

            if (!this.completionUrl.endsWith('/')) {
                this.completionUrl += '/';
            }
        }

        $(this.element).on('click', '.' + this.classes.deleteItem, (ev) => {
            if (ev.target) {
                let $item = $(ev.target).closest('.' + this.classes.item);
                let $closest = $(ev.target).closest('[data-item-id]');
                let id = parseInt($closest.attr('data-item-id') || '-1');
                if (id > 0) {
                    this.removeItem(this.getItemIndex(id));
                } else {
                    this.removeItem(this.getItemIndex($closest.text()));
                }
                $item.remove();
            }
        });

        this.load();
    }
    
    save(): void {
        this.saveElement.val(JSON.stringify(this.items));
    }

    load(): void {
        $('.' + this.classes.item, this.element).remove();
        let json = this.saveElement.val() as string;
        if (json && json.length > 0) {
            this.items = JSON.parse(json);
            for (let i = 0; i < this.items.length; i++) {
                this.renderItem(this.items[i]);
            }
        }
    }

    renderItem(item: any): void {
        let $item = $('<span></span>');
        $item.addClass(this.classes.item);
        $item.attr('data-item-id', item.value);
        $item.text(item.label);
        let $close = $('<i class="fas fa-times ml-3"></i>');
        $close.addClass(this.classes.deleteItem);
        $item.append($close);
        $(this.searchFieldElement).before($item)
    }

    addItem(item: any): void {
        if (typeof item === 'string') {
            item = { label: item.toLowerCase(), value: -1 };
        } else if (typeof item.value === 'string') {
            item.value = parseInt(item.value);
        }

        this.items.push(item);
        this.save();
        this.renderItem(item);
    }

    removeItem(item: any): void {
        this.items.splice(item, 1);
        this.save();
    }

    getItemIndex(item: any): any {
        if (typeof item === 'string') {
            for (let i = 0; i < this.items.length; i++) {
                if (this.items[i].label === item) {
                    return i;
                }
            }
        } else if (typeof item === 'number') {
            for (let i = 0; i < this.items.length; i++) {
                if (this.items[i].value === item) {
                    return i;
                }
            }
        }
        return null;
    }

    select(ev: Event, item: any): void {
        ev.preventDefault();
        ev.stopImmediatePropagation();
        this.addItem(item.item);
        this.searchFieldElement.value = '';
    }

    source(request: any, response: any): void {
        if (request && request.term && request.term.trim().length > 0) {
            let separatedItems = request.term.split(/[^a-zA-Z0-9_]/);
            if (separatedItems.length > 0 && separatedItems[0].length === 0) {
                separatedItems.splice(0);
            }
            
            // console.log(separatedItems);
            if (separatedItems.length > 0) {
                for (let i = 0; i < separatedItems.length - 1; i++) {
                    this.addItem(separatedItems[i]);
                }
                this.searchFieldElement.value = (request.term = separatedItems[separatedItems.length - 1]);
            }
            
            $.ajax({
                method: 'GET',
                url: this.completionUrl + request.term,
                dataType: 'json'
            }).then(r => response(r.items), () => response([]));
        } else {
            response([]);
        }
    }
};

$(() => {
    let $tagEditor = $('.js-tag-editor');
    if ($tagEditor.length > 0) {
        new TagEditor($tagEditor, undefined);
    }

    let $userHandleEditor = $('.js-user-handle-editor');
    if ($userHandleEditor.length > 0) {
        new UserHandleEditor($userHandleEditor, undefined);
    }
});

class TagEditor extends ItemEditor {
    constructor(el: any, options: any) {
        super(el, $.extend({
            classes: {
                editor: 'js-tag-editor',
                search: 'js-search-field',
                persist: 'js-tags',
                url: 'js-tag-editor--url',
                deleteItem: 'js-tag-editor--delete-tag',
                item: 'js-tag-editor--tag',
            }
        }, options));
    }
}

class UserHandleEditor extends ItemEditor {
    constructor(el: any, options: any) {
        super(el, $.extend({
            classes: {
                editor: 'js-user-handle-editor',
                search: 'js-search-field',
                persist: 'js-user-handles',
                url: 'js-user-handle-editor--url',
                deleteItem: 'js-user-handle-editor--delete',
                item: 'js-user-handle-editor--item',
            }
        }, options));
    }
}