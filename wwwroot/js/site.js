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
            dataType: responseType,
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
            $input.val($input.attr('data-debug-value'));
        }
    });
});