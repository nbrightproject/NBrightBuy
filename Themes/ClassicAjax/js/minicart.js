
$(document).ready(function () {

    //Form validation 
    var form = $("#Form");
    form.validate();

    $('.addtobasket').click(function () {
        alert('why');
        if (form.valid()) {
            $('.processing').show();
            if (parseInt($('.quantity').val()) < 1) $('.quantity').val('1');
            nbxget('cart_addtobasket', '.entryid' + $(this).attr('itemid'), '#minicartdatareturn'); // Reload Cart
            $('.addedtobasket').delay(10).fadeIn('fast');
        }
    });


    $(document).on("nbxgetcompleted", NBS_MiniCart_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function NBS_MiniCart_nbxgetCompleted(e) {

        if (e.cmd == 'cart_addtobasket') {
            nbxget('cart_renderminicart', '.minicartdata', '.container_classicajax_nbs_minicart'); // Reload Cart
            $('.addedtobasket').delay(10).fadeOut('fast');
        }

        if (e.cmd == 'cart_renderminicart') {
            $('.processing').hide();

        }
    }

});
