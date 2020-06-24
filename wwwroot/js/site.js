// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(() => {
    $(document).on('click', '.contains-goto', (ev) => {
        let href = $(ev.target).closest('.contains-goto').find('a.goto').attr('href');
        window.location.assign(href);
    });

    $(document).on('click', '.contains-ajax', (ev) => {
        ev.preventDefault();
        let $parent = $(ev.target).closest('.contains-ajax');
        let $ajax = $parent.find('a.ajax');
        let href = $ajax.attr('href');
        let method = $ajax.attr('data-method') || 'POST';
        let responseHandler = $ajax.attr('data-response-handler');
        let responseType = $ajax.attr('data-response-type');
        let responseOnClosest = $ajax.attr('data-response-on-closest');

        let p = $.ajax({
            url: href,
            type: method,
            dataType: responseType || 'html',
        });
        if (responseHandler) {
            if (responseOnClosest && responseOnClosest.trim().length > 0) {
                p.then(r => $ajax.closest(responseOnClosest).trigger(responseHandler, [r, $ajax]));
            } else {
                p.then(r => $ajax.trigger(responseHandler, [r, $ajax]));
            }
        }
        return false;
    });

    $(document).on('replacewith', '.contains-ajax', (ev, r, $ajax) => {
        $ajax.closest('.contains-ajax')[0].outerHTML = r;
    });

    $(document).on('page-reload', '.contains-ajax', (ev, r, $ajax) => {
        ev.preventDefault();
        location.reload();
    });

    $(document).on('post-replacewith', '.post', (ev, r, $ajax) => {
        let $post = $ajax.closest('.post');
        let $newElement = $(r);
        $newElement.css('height', $post[0].clientHeight + 'px')
        $post.after($newElement);
        $post.remove();
    });

    $(document).on('archived-media-replacewith', '.archived-media', (ev, r, $ajax) => {
        let $post = $ajax.closest('.archived-media');
        let $newElement = $(r);
        $newElement.css('height', $post[0].clientHeight + 'px')
        $post.after($newElement);
        $post.remove();
    });

    $(document).on('click', 'form button.js-debug-values', (ev) => {
        let $form = $(ev.target).closest('form');
        let $inputs = $('input[data-debug-value],select[data-debug-value],textarea[data-debug-value]');
        for (let i = 0; i < $inputs.length; i++) {
            let $input = $($inputs.get(i));
            let newVal = $input.attr('data-debug-value').trim();
            if (newVal.length > 0) {
                $input.val(newVal);
                if ($input.is('.js-tags')) {
                    CachedTagEditors[$input.closest('.js-tag-editor').attr('data-tag-editor-id')].load();
                }
            }
        }
    });

    let jsKeypressValidateTimeoutHandle = null;
    let $jsKeypressValidateTimeoutHandleError = null;
    $("input.js-keypress-validate").keyup((ev) => {
        if (jsKeypressValidateTimeoutHandle) {
            clearTimeout(jsKeypressValidateTimeoutHandle);
        }
        if ($jsKeypressValidateTimeoutHandleError) {
            $jsKeypressValidateTimeoutHandleError.remove();
            $jsKeypressValidateTimeoutHandleError = null;
        }
        jsKeypressValidateTimeoutHandle = setTimeout(() => {
            if (!ev.target.classList.contains('input-validation-error')) {
                $.ajax({
                    url: ev.target.dataset.valRemoteUrl + '?' + ev.target.name + '=' + encodeURIComponent(ev.target.value),
                    dataType: 'json'
                }).then(r => {
                    if (r !== true) {
                        $jsKeypressValidateTimeoutHandleError = $('<span class="text-danger"></span>');
                        $jsKeypressValidateTimeoutHandleError.text(ev.target.dataset.valRemote);
                        $(ev.target).parent().after($jsKeypressValidateTimeoutHandleError);
                    }
                });
            }
        }, 500);
    });

    $(document).on('notification-dismiss', '.js-notification-container', (ev, r, $ajax) => {
        let $container = $ajax.closest('.js-notification-container');
        $container.remove();
    });

    let $tagEditor = $('.js-tag-editor');
    if ($tagEditor.length > 0) {
        new TagEditor($tagEditor);
    }
});


TagEditorId = 0;
CachedTagEditors = {};

class TagEditor {
    constructor(element) {
        this.id = TagEditorId;
        TagEditorId++;
        CachedTagEditors[this.id] = this;

        this.tags = [];
        this.element = (typeof element === 'string' || element === undefined) ? $(element || '.js-tag-editor') : element;
        this.searchFieldElement = $('.js-search-field', this.element);
        this.saveElement = $('.js-tags', this.element);

        this.element.attr('data-tag-editor-id', this.id);
        this.completionUrl = this.element.find('.js-tag-editor--url').prop('href');
        if (!this.completionUrl.endsWith('/')) {
            this.completionUrl += '/';
        }
        
        this.searchFieldElement.autocomplete({
            minLength: 0,
            source: this.source.bind(this),
            select: this.select.bind(this),
        });

        this.element.on('click', '.js-tag-editor--delete-tag', (ev) => {
            let $tag = $(ev.target).closest('.js-tag-editor--tag');
            let $closest = $(ev.target).closest('[data-tag-id]');
            let id = parseInt($closest.attr('data-tag-id'));
            if (id > 0) {
                this.removeTag(this.getTagIndex(id));
            } else {
                this.removeTag(this.getTagIndex($closest.text()));
            }
            $tag.remove();
        });

        this.load();
    }

    save() {
        this.saveElement.val(JSON.stringify(this.tags));
    }

    load() {
        $('.js-tag-editor--tag', this.element).remove();
        let json = this.saveElement.val();
        if (json && json.length > 0) {
            this.tags = JSON.parse(json);
            for (let i = 0; i < this.tags.length; i++) {
                this.renderTag(this.tags[i]);
            }
        }
    }

    renderTag(tag) {
        let $tag = $('<span class="js-tag-editor--tag"></span>');
        $tag.attr('data-tag-id', tag.value);
        $tag.text(tag.label);
        $tag.append($('<i class="fas fa-times ml-3 js-tag-editor--delete-tag"></i>'));
        $(this.searchFieldElement).before($tag)
    }

    addTag(tag) {
        if (typeof tag === 'string') {
            tag = { label: tag.toLowerCase(), value: -1 };
        } else if (typeof tag.value === 'string') {
            tag.value = parseInt(tag.value);
        }

        this.tags.push(tag);
        this.save();
        this.renderTag(tag);
    }

    removeTag(tag) {
        this.tags.splice(tag, 1);
        this.save();
    }

    getTagIndex(tag) {
        if (typeof tag === 'string') {
            for (let i = 0; i < this.tags.length; i++) {
                if (this.tags[i].label === tag) {
                    return i;
                }
            }
        } else if (typeof tag === 'number') {
            for (let i = 0; i < this.tags.length; i++) {
                if (this.tags[i].value === tag) {
                    return i;
                }
            }
        }
        return null;
    }

    select(ev, tag) {
        ev.preventDefault();
        ev.stopImmediatePropagation();
        this.addTag(tag.item);
        this.searchFieldElement.val('');
    }

    source(request, response) {
        if (request && request.term && request.term.trim().length > 0) {
            let separatedTags = request.term.split(/[^a-zA-Z0-9_]/);
            if (separatedTags.length > 0 && separatedTags[0].length === 0) {
                separatedTags.splice(0);
            }
            
            console.log(separatedTags);
            if (separatedTags.length > 0) {
                for (let i = 0; i < separatedTags.length - 1; i++) {
                    this.addTag(separatedTags[i]);
                }
                this.searchFieldElement.val(request.term = separatedTags[separatedTags.length - 1]);
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
}