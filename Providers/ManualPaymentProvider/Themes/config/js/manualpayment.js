

$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file

    $('#manualpayment_cmdSave').unbind("click");
    $('#manualpayment_cmdSave').click(function () {
        $('.processing').show();
        $(this).hide();
        nbxget('manualpaymentajax_savesettings', '.manualpaymentdata', '.manualpaymentreturnmsg');
    });


    $(document).on("nbxgetcompleted", NBS_PayBox_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function NBS_PayBox_nbxgetCompleted(e) {

        $('.processing').hide();

        $('#manualpayment_cmdSave').show();

    };

});

