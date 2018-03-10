
$(document).ready(function () {

    
    $('#cmdDeleteCart').click(function () {
        var msg = $('#cmdClearCart').val();
        if (confirm(msg)) {
            $('.processing').show();
            nbxget('clearcart', '#fullcartdata', '#fullcartdatareturn');
        }
    });

    $('#cmdRecalcCart').click(function () {
        $('.processing').show();
        $('#cmdGoCheckout').show();
        nbxget('cart_recalculatecart', '.cartdatarow', '#fullcartdatareturn', '.quantitycolumn');
    });

    $('#cmdGoCheckout').click(function () {
        $('.processing').show();
        nbxget('cart_recalculatecart', '.cartdatarow', '#fullcartredirectreturn', '.quantitycolumn');
    });

    // Ajax action return, reload cart list
    $('#fullcartdatareturn').change(function () {
        nbxget('rendercart', '#fullcartdata', '#checkoutitemlist');
    });

    // Ajax redirect action return, redirect to checkout
    $('#fullcartredirectreturn').change(function () {
        $('.processing').show();
        var redirecturl = $('#checkouturl').val();
        window.location.href = redirecturl + '?cartstep=2';
    });

    // cart list loaded
    $('#checkoutitemlist').change(function () {

        $('.removeitem').unbind();
        $('.removeitem').click(function () {
            $('.processing').show();
            $('#itemcode').val($(this).attr('itemcode'));
            nbxget('removefromcart', '#fullcartdata', '#fullcartdatareturn');
        });

        $('.processing').hide();

        // if we have a cartempty element hide the action buttons
        if ($('#cartempty').text() != '') {
            $('#cartdetails').hide();
        } else {
            $('#cartdetails').show();
        }

   
        $(".quantity").keydown(function (e) {
            if (e.keyCode == 8 || e.keyCode <= 46) return; // Allow: backspace, delete.
            if ((e.keyCode >= 35 && e.keyCode <= 39)) return; // Allow: home, end, left, right
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) e.preventDefault();
        });

        $('.qtyminus').unbind();
        $('.qtyminus').click(function () {
            var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
            var newQty = parseInt(oldqty, 10);
            if (isNaN(newQty)) {
                newQty = 2;
            }
            if (newQty >= 1) {
                --newQty;
                $('.itemcode' + $(this).attr('itemcode')).val(newQty);
            }
        });
        $('.qtyplus').unbind();
        $('.qtyplus').click(function () {
            var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
            var newQty = parseInt(oldqty, 10);
            if (isNaN(newQty)) {
                newQty = 0;
            }
            ++newQty;
            $('.itemcode' + $(this).attr('itemcode')).val(newQty);
        });

    });

    // show cart list
    $('.processing').show();
    nbxget('rendercart', '#fullcartdata', '#checkoutitemlist');


});
