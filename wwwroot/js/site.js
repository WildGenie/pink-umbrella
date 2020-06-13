// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


$(() => {
    $('.contains-goto').click((ev) => {
        let href = $(ev.target).closest('.contains-goto').find('a.goto').attr('href');
        window.location.assign(href);
    })
});