

$(document).ready(function () {
    // start load all ajax data, continued by js in product.js file
    $('.processing').show();
    $('#razortemplate').val('NBS_AddressAdminList.cshtml');
    nbxget('addressadmin_getlist', '#nbs_addressadminsearch', '#datadisplay');

    $(document).on("nbxgetcompleted", NBS_AddressAdmin_nbxgetCompleted); // assign a completed event for the ajax calls

    // function to do actions after an ajax call has been made.
    function NBS_AddressAdmin_nbxgetCompleted(e) {

        $('.processing').hide();

        if (e.cmd == 'addressadmin_getlist') {

            $('.AddressAdmin_cmdEdit').unbind("click");
            $('.AddressAdmin_cmdEdit').click(function () {
                $('.processing').show();
                $('#razortemplate').val('NBS_AddressAdminDetail.cshtml');
                $('#selecteditemid').val($(this).attr("itemid"));
                $('#selectedindex').val($(this).attr("index"));                
                nbxget('addressadmin_editaddress', '#nbs_addressadminsearch', '#datadisplay');
            });

            $('.AddressAdmin_cmdDel').unbind("click");
            $('.AddressAdmin_cmdDel').click(function () {
                $('.processing').show();
                $('#razortemplate').val('NBS_AddressAdminList.cshtml');
                nbxget('addressadmin_delete', '#nbs_addressadminsearch', '#datadisplay');
            });

            $('.AddressAdmin_cmdAdd').unbind("click");
            $('.AddressAdmin_cmdAdd').click(function () {
                $('.processing').show();
                $('#razortemplate').val('NBS_AddressAdminList.cshtml');
                nbxget('addressadmin_add', '#nbs_addressadminsearch', '#datadisplay');
            });

        }

        if (e.cmd == 'addressadmin_getdetail') {


        }



    };

});

