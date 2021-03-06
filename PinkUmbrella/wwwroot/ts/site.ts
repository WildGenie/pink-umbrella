declare var $: any;

$(() => {
    $(document).on('click', '.contains-goto', (ev: Event, data?: any) => {
        let href = $(ev.target).closest('.contains-goto').find('a.goto').attr('href');
        if (href) {
            window.location.assign(href);
        }
    });

    $(document).on('click', '.contains-goto', (ev: Event, data?: any) => {
        let href = $(ev.target).closest('.contains-goto').find('a.goto').attr('href');
        if (href) {
            window.location.assign(href);
        }
    });

    $(document).on('replacewith', (ev, r, $ajax) => {
        if ($ajax)
        {
            $ajax.closest('.contains-ajax')[0].outerHTML = r;
        }
    });

    $(document).on('page-reload', '.contains-ajax', (ev: Event) => {
        ev.preventDefault();
        location.reload();
    });

    document.addEventListener('post-replacewith', (ev) => {
        console.log((ev as any).detail.r);
        console.log((ev as any).detail);
        if (ev.target && (<HTMLElement>ev.target).classList.contains('post')) {
            let $post = $(ev.target);
            let $newElement = $((ev as any).detail.r);
            $newElement.css('height', $post[0].clientHeight + 'px')
            $post.after($newElement);
            $post.remove();
        }
    });

    $(document).on('archived-media-replacewith', '.archived-media', (ev: Event) => {
        let $post = (ev as any).data.$ajax.closest('.archived-media');
        let $newElement = $((ev as any).data.r);
        $newElement.css('height', $post[0].clientHeight + 'px')
        $post.after($newElement);
        $post.remove();
    });

    $(document).on('click', 'form button.js-debug-values', (ev: Event) => {
        let $form = $(ev.target).closest('form');
        let $inputs = $('input[data-debug-value],select[data-debug-value],textarea[data-debug-value]');
        for (let i = 0; i < $inputs.length; i++) {
            let $input = $($inputs.get(i));
            let newVal = ($input.attr('data-debug-value') as string).trim();
            if (newVal.length > 0) {
                $input.val(newVal);
                if ($input.is('.js-tags')) {
                    // TODO: uncomment
                    // CachedTagEditors[$input.closest('.js-tag-editor').attr('data-tag-editor-id')].load();
                }
            }
        }
    });

    $(document).on('change', '.js-contains-selectable-goto', (ev: Event) => {
        let $select = $(ev.target);
        let $gotos = $($select.attr('data-selectable-gotos')).find('a');
        let gotoIndex = parseInt($select.val());
        let $goto = $gotos[gotoIndex];
        if ($goto) {
            $goto.click();
        }
    });

    let jsKeypressValidateTimeoutHandle: any | undefined = undefined;
    let $jsKeypressValidateTimeoutHandleError: any | undefined = undefined;
    $("input.js-keypress-validate").keyup((ev) => {
        if (jsKeypressValidateTimeoutHandle) {
            clearTimeout(jsKeypressValidateTimeoutHandle);
        }
        if ($jsKeypressValidateTimeoutHandleError) {
            $jsKeypressValidateTimeoutHandleError.remove();
            $jsKeypressValidateTimeoutHandleError = undefined;
        }
        jsKeypressValidateTimeoutHandle = setTimeout(() => {
            if (!ev.target.classList.contains('input-validation-error')) {
                let $input = ev.target as HTMLInputElement;
                $.ajax({
                    url: ev.target.dataset.valRemoteUrl + '?' + $input.name + '=' + encodeURIComponent($input.value),
                    dataType: 'json'
                }).then(r => {
                    if (r !== true) {
                        $jsKeypressValidateTimeoutHandleError = $('<span class="text-danger"></span>');
                        $jsKeypressValidateTimeoutHandleError.text(ev.target.dataset.valRemote || '');
                        $(ev.target).parent().after($jsKeypressValidateTimeoutHandleError);
                    }
                });
            }
        }, 500);
    });

    $(document).on('notification-dismiss', '.js-notification-container', (ev: Event) => {
        let $container = (<any>ev).data.$ajax.closest('.js-notification-container');
        $container.remove();
    });
    
    $(".simple-table-filter-input").on("keyup", function () {
        var $this = $(this);
        var value = ($this.val() as string || '').toLowerCase();
        var tableSelector = $this.attr('data-el-selector');
        $(tableSelector + ' tbody tr').each(function (index: number, element: HTMLElement): void {
            $(this).toggle(($(this).text().toLowerCase() || '').indexOf(value) > -1)
        });
    });
    
    $(document).on('change', '.instant-dropdown-post select', function () {
        let $form = $(this).closest('form');
        $form.submit();
    });

    $(document).on('change', '.instant-input-post input', function () {
        let $form = $(this).closest('form');
        $form.submit();
    });

    let inputTimeouts: any[] = [];
    $(document).on('change keyup paste', '.input-auto-post', function () {
        let $input = $(this);
        let timeoutIdStr = $input.attr('data-timeout-id') || '';
        let timeoutId: number;
        if (timeoutIdStr.length > 0) {
            timeoutId = parseInt(timeoutIdStr);
        
            if (inputTimeouts[timeoutId]) {
                clearTimeout(inputTimeouts[timeoutId]);
            }
        } else {
            timeoutId = inputTimeouts.length;
            $input.attr('data-timeout-id', timeoutId);
            inputTimeouts.push(null);
        }

        if (!$input.next().is('.badge')) {
            $input.after('<span class="badge validation-badge"><span class="success"><i class="fa fa-check"></i></span><span class="danger"><i class="fa fa-times"></i></span><span class="warning"><i class="fa fa-exclamation"></i></span><span class="thinking"><i class="fa fa-sync"></i></span></span>');
        }

        $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
        $input.addClass('validate-thinking');

        let $form = $input.closest('form');
        let action = $form.attr('action');
        if (action && action !== undefined) {
            inputTimeouts[timeoutId] = setTimeout(() => {
                inputTimeouts[timeoutId] = null;
                let data = $form.serialize();
                let dfd = null;
                if ($form.attr('method') == 'post') {
                    dfd = $.post(action as string, data);
                } else {
                    dfd = $.get(action as string, data);
                }

                dfd.then(() => {
                    $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
                    $input.addClass('validate-success');
                }, () => {
                    console.error('could not post');
                    $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
                    $input.addClass('validate-danger');
                });
            }, 1000);
        }
    });

    $(document).on('focusout, change, keydown, keyup', '.ensure-ready-input input', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        let $lastInput = $inputs.last();
        if (($lastInput.val() as string || '').trim() !== '') {
            let $newInput = $($lastInput[0].outerHTML);
            $newInput.val('');
            $lastInput.after($newInput);
        } else {
            for (var i = 0; i < $inputs.length - 1; i++) {
                if (($($inputs[i]).val() as string || '').trim() === '') {
                    $inputs[i].remove();
                }
            }
        }
    });

    $(document).on('click', '.ensure-ready-input button.btn-upload', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        let $lastInput = $inputs.last();

        let $file = $('<input type="file" />');
        $file.css('display', 'none');
        $('body').append($file);

        $file.on('change', function () {
            let files = (<HTMLInputElement>$file[0]).files;
            if (files && files.length === 1) {
                files[0].text().then((csvString: string) => {
                    $file.remove();
                    let labels = csvString.split('\n');
                    let newInputTemplate = $lastInput[0].outerHTML;
                    for (var label of labels) {
                        let $newInput = $(newInputTemplate);
                        $newInput.val(label);
                        $lastInput.after($newInput);
                        $lastInput = $newInput;
                    }
                });
            }
        });
        $file.on('cancel', () => $file.remove());
        $file.click();
    });

    $(document).on('click', '.ensure-ready-input button.btn-clear', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        for (var i = 0; i < $inputs.length - 1; i++) {
            $inputs[i].remove();
        }
        $inputs.last().val('');
    });


    //$(document).on('click', 'nav.accordion,nav.accordion .accordion', function () {
    //    let $this = $(this);
    //});

    $(document).on('click', '.click2edit', function (ev: Event) {
        let $this = $(this);
        if (!$this.hasClass('editing')) {
            $this.toggleClass('editing');
            let $input = $('input', $this);
            let ta = (<HTMLTextAreaElement>$input[0]);
            $input.focus();
            setTimeout(() => ta.selectionStart = ta.selectionEnd = 9999, 0);
            $input.attr('data-click2edit-initial-val', $input.val() as string || '');
        }
    });

    $(document).on('focusout', '.click2edit input', function (ev: Event) {
        let $this = $(this);
        let $click2edit = $this.closest('.click2edit');
        if ($click2edit.hasClass('editing')) {
            $this.val($this.attr('data-click2edit-initial-val') as string || '');
            $click2edit.toggleClass('editing');
        }
    });

    $(document).on('dom-changed', '.has-on-empty-message', function (ev) {
        let $this = $(this);
        let isEmpty = $this.children(':not(.on-empty)').length === 0;
        if (isEmpty) {
            $this.addClass('is-empty');
        } else {
            $this.removeClass('is-empty');
        }
    });


    $(document).on('click', '.contains-ajax', (ev: Event, data?: any) => {
        ev.preventDefault();
        let $parent = $(ev.target).closest('.contains-ajax');
        let $ajax = $parent.find('a.ajax');
        if ($ajax.attr('disabled')) {
            return false;
        } else {
            $ajax.prop('disabled', true);
        }
        let href = $ajax.attr('href');
        let method = $ajax.attr('data-method') || 'POST';
        let responseHandler = $ajax.attr('data-response-handler') as string;
        let responseType = $ajax.attr('data-response-type') || 'html';
        let responseOnClosest = $ajax.attr('data-response-on-closest') || '';
        
        let p = $.ajax({
            url: href,
            type: method,
            dataType: responseType,
        });
        if (responseHandler) {
            var replaceEv = (r) => new CustomEvent(responseHandler, { detail: { r, $ajax } });
            // if (responseOnClosest && responseOnClosest.trim().length > 0) {
            //     p.then(r => $ajax.closest(responseOnClosest)[0].dispatchEvent(replaceEv(r)));
            // } else {
            //     p.then(r => $ajax[0].dispatchEvent(replaceEv(r)));
            // }
            if (responseOnClosest && responseOnClosest.trim().length > 0) {
                p.then(r => $ajax.closest(responseOnClosest).trigger(responseHandler, [ r, $ajax ]));
            } else {
                p.then(r => $ajax.trigger(responseHandler, [ r, $ajax ]));
            }
        }
        p.then(r => $ajax.prop('disabled', false));
        return false;
    });

    $(document).on('page-reload', '.contains-ajax', (ev: Event) => {
        ev.preventDefault();
        location.reload();
    });

    $(document).on('post-replacewith', '.post', (ev: Event) => {
        console.log((ev as any).detail);
        console.log((ev as any).detail.r);
        let $post = (ev as any).detail.$ajax.closest('.post');
        let $newElement = $((ev as any).detail.r);
        $newElement.css('height', $post[0].clientHeight + 'px')
        $post.after($newElement);
        $post.remove();
    });

    $(document).on('archived-media-replacewith', '.archived-media', (ev: Event) => {
        let $post = (ev as any).data.$ajax.closest('.archived-media');
        let $newElement = $((ev as any).data.r);
        $newElement.css('height', $post[0].clientHeight + 'px')
        $post.after($newElement);
        $post.remove();
    });

    $(document).on('click', 'form button.js-debug-values', (ev: Event) => {
        let $form = $((<any>ev).target).closest('form');
        let $inputs = $('input[data-debug-value],select[data-debug-value],textarea[data-debug-value]');
        for (let i = 0; i < $inputs.length; i++) {
            let $input = $($inputs.get(i));
            let newVal = ($input.attr('data-debug-value') || '').trim();
            if (newVal.length > 0) {
                $input.val(newVal);
                if ($input.is('.js-tags')) {
                    // TODO: uncomment
                    // CachedTagEditors[$input.closest('.js-tag-editor').attr('data-tag-editor-id')].load();
                }
            }
        }
    });

    $("input.js-keypress-validate").on('keyup', (ev: Event): void => {
        if (jsKeypressValidateTimeoutHandle) {
            clearTimeout(jsKeypressValidateTimeoutHandle);
        }
        if ($jsKeypressValidateTimeoutHandleError) {
            $jsKeypressValidateTimeoutHandleError.remove();
            $jsKeypressValidateTimeoutHandleError = undefined;
        }
        jsKeypressValidateTimeoutHandle = setTimeout(() => {
            let target = (ev as InputEvent).target as HTMLInputElement;
            if (target && !target.classList.contains('input-validation-error')) {
                $.ajax({
                    url: target.dataset.valRemoteUrl + '?' + target.name + '=' + encodeURIComponent(target.value),
                    dataType: 'json'
                }).then(r => {
                    if (r !== true) {
                        $jsKeypressValidateTimeoutHandleError = $('<span class="text-danger"></span>');
                        $jsKeypressValidateTimeoutHandleError.text(target.dataset.valRemote || '');
                        $(target).parent().after($jsKeypressValidateTimeoutHandleError);
                    }
                });
            }
        }, 500);
    });

    $(document).on('notification-dismiss', '.js-notification-container', (ev: Event) => {
        let $container = (<any>ev).data.$ajax.closest('.js-notification-container');
        $container.remove();
    });
    
    $(".simple-table-filter-input").on("keyup", function () {
        var $this = $(this);

        var value = ($this.val() as string || '').toLowerCase();
        var tableSelector = $this.attr('data-el-selector');
        $(tableSelector + ' tbody tr').each(function () {
            $(this).toggle(($(this).text().toLowerCase() || '').indexOf(value) > -1)
        });
    });
    $(document).on('change', '.instant-dropdown-post select', function () {
        let $form = $(this).closest('form');
        $form.submit();
    });

    $(document).on('change', '.instant-input-post input', function () {
        let $form = $(this).closest('form');
        $form.submit();
    });

    $(document).on('change keyup paste', '.input-auto-post', function () {
        let $input = $(this);
        let timeoutIdStr = $input.attr('data-timeout-id') || '';
        let timeoutId: number;
        if (timeoutIdStr.length > 0) {
            timeoutId = parseInt(timeoutIdStr);
        
            if (inputTimeouts[timeoutId]) {
                clearTimeout(inputTimeouts[timeoutId]);
            }
        } else {
            timeoutId = inputTimeouts.length;
            $input.attr('data-timeout-id', timeoutId);
            inputTimeouts.push(null);
        }

        if (!$input.next().is('.badge')) {
            $input.after('<span class="badge validation-badge"><span class="success"><i class="fa fa-check"></i></span><span class="danger"><i class="fa fa-times"></i></span><span class="warning"><i class="fa fa-exclamation"></i></span><span class="thinking"><i class="fa fa-sync"></i></span></span>');
        }

        $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
        $input.addClass('validate-thinking');

        let $form = $input.closest('form');
        inputTimeouts[timeoutId] = setTimeout(() => {
            inputTimeouts[timeoutId] = null;
            let data = $form.serialize();
            let dfd = null;
            let action = $form.attr('action');
            if (action && action !== undefined) {
                if ($form.attr('method') == 'post') {
                    dfd = $.post(action, data);
                } else {
                    dfd = $.get(action, data);
                }

                dfd.then((response: any) => {
                    $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
                    $input.addClass('validate-success');
                }, () => {
                    console.error('could not post');
                    $input.removeClass('validate-success validate-danger validate-warning validate-thinking');
                    $input.addClass('validate-danger');
                });
            }
        }, 1000);
    });

    $(document).on('focusout, change, keydown, keyup', '.ensure-ready-input input', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        let $lastInput = $inputs.last();
        if ($lastInput && ($lastInput.val() as string || '').trim() !== '') {
            let $newInput = $($lastInput[0].outerHTML);
            $newInput.val('');
            $lastInput.after($newInput);
        } else {
            for (var i = 0; i < $inputs.length - 1; i++) {
                if (($($inputs[i]).val() as string || '').trim() === '') {
                    $inputs[i].remove();
                }
            }
        }
    });

    $(document).on('click', '.ensure-ready-input button.btn-upload', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        let $lastInput = $inputs.last();

        let $file = $('<input type="file" />');
        $file.css('display', 'none');
        $('body').append($file);

        $file.on('change', function () {
            let files = (<HTMLInputElement>$file[0]).files;
            if (files && files.length === 1) {
                files[0].text().then((csvString: string) => {
                    $file.remove();
                    let labels = csvString.split('\n');
                    let newInputTemplate = $lastInput[0].outerHTML;
                    for (var label of labels) {
                        let $newInput = $(newInputTemplate);
                        $newInput.val(label);
                        $lastInput.after($newInput);
                        $lastInput = $newInput;
                    }
                });
            }
        });
        $file.on('cancel', () => $file.remove());
        $file.click();
    });

    $(document).on('click', '.ensure-ready-input button.btn-clear', function () {
        let $ensureReadyInput = $(this).closest('.ensure-ready-input');
        let $inputs = $('input', $ensureReadyInput);
        for (var i = 0; i < $inputs.length - 1; i++) {
            $inputs[i].remove();
        }
        $inputs.last().val('');
    });


    //$(document).on('click', 'nav.accordion,nav.accordion .accordion', function () {
    //    let $this = $(this);
    //});

    $(document).on('click', '.click2edit', function (ev: Event) {
        let $this = $(this);
        if (!$this.hasClass('editing')) {
            $this.toggleClass('editing');
            let $input = $('input', $this);
            $input.focus();
            setTimeout(() => (<HTMLTextAreaElement>$input[0]).selectionStart = (<HTMLTextAreaElement>$input[0]).selectionEnd = 9999, 0);
            $input.attr('data-click2edit-initial-val', $input.val() as string || '');
        }
    });

    $(document).on('focusout', '.click2edit input', function (ev: Event) {
        let $this = $(this);
        let $click2edit = $this.closest('.click2edit');
        if ($click2edit.hasClass('editing')) {
            $this.val($this.attr('data-click2edit-initial-val') || '');
            $click2edit.toggleClass('editing');
        }
    });

    $(document).on('dom-changed', '.has-on-empty-message', function (ev: Event) {
        let $this = $(this);
        let isEmpty = $this.children(':not(.on-empty)').length === 0;
        if (isEmpty) {
            $this.addClass('is-empty');
        } else {
            $this.removeClass('is-empty');
        }
    });
});