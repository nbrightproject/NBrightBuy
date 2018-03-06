

$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file

    $(document).on("nbxgetcompleted", NBS_Payment_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function NBS_Payment_nbxgetCompleted(e) {

        $('.processing').hide();

        if (e.cmd == 'payment_getlist') {

            $('.processing').show();
            $('#razortemplate').val('NBS_PaymentList.cshtml');
            nbxget('payment_getlist', '#nbs_Paymentsearch', '#datadisplay');

        }

    };

});

