$(document).ready(function () {

    $(".quantity").keydown(function (e) {
        if (e.keyCode == 8 || e.keyCode <= 46) return; // Allow: backspace, delete.
        if ((e.keyCode >= 35 && e.keyCode <= 39)) return; // Allow: home, end, left, right
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) e.preventDefault();
    });

    $('.qtyminus').click(function () {
        if (parseInt($('.quantity').val()) > 1)
            $('.quantity').val(parseInt($('.quantity').val()) - 1);
        else
            $('.quantity').val('1');
    });

    $('.qtyplus').click(function () {
        $('.quantity').val(parseInt($('.quantity').val()) + 1);
        if ($('.quantity').val() == 'NaN') $('.quantity').val('1');
    });

    $('.buybutton').click(function () {
        if (parseInt($('.quantity').val()) < 1) $('.quantity').val('1');
    });


});
