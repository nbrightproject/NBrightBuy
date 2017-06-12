$(document).ready(function() {

    $(document).on("nbxgetcompleted", Admin_product_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_ProductsList.cshtml');
    nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_product_nbxgetCompleted(e) {

        //NBS - Tooltips
        $('[data-toggle="tooltip"]').tooltip({
            animation: 'true',
            placement: 'auto top',
            viewport: { selector: '#content', padding: 0 },
            delay: { show: 100, hide: 200 }
        });


        if (e.cmd == 'product_admin_getlist') {

            $('.processing').hide();

            // Move products
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").hide();
            $(".savebutton").hide();

            $("#ddllistsearchcategory").unbind("change");
            $("#ddllistsearchcategory").change(function() {
                $('#searchcategory').val($("#ddllistsearchcategory").val());
                $(".selectrecord").show();
                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });

            $("#chkcascaderesults").unbind("change");
            $("#chkcascaderesults").change(function() {
                if ($("#chkcascaderesults").is(':checked')) {
                    $('#cascade').val("True");
                } else {
                    $('#cascade').val("False");
                }
                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });


            $('.selectrecord').unbind("click");
            $(".selectrecord").click(function() {
                $(".selectrecord").hide();
                $(".selectmove").show();
                $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
                $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
                $("#moveproductid").val($(this).attr("itemid"));

                var selectid = $(this).attr("itemid");
                $(".selectmove").each(function(index) {
                    if ($(this).attr("parentlist").indexOf(selectid + ";") > -1) $(this).hide();
                });

            });

            $('.selectcancel').unbind("click");
            $(".selectcancel").click(function() {
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
                $("#searchcategory").val('');
            });

            $('.selectmove').unbind("click");
            $(".selectmove").click(function() {
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
                $("#movetoproductid").val($(this).attr("itemid"));
                nbxget('product_moveproductadmin', '#nbs_productadminsearch', '#datadisplay');
            });

            $("#productAdmin_cmdSave").hide();
            $("#productAdmin_cmdReturn").hide();

            $('#productAdmin_searchtext').val($('#searchtext').val());

            // editbutton created by list, so needs to be assigned on each render of list.
            $('#productAdmin_cmdEdit').unbind("click");
            $('#productAdmin_cmdEdit').click(function() {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductsDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('product_admin_getdetail', '#nbs_productadminsearch', '#datadisplay');
            });

            $('.cmdPg').unbind("click");
            $('.cmdPg').click(function() {
                $('.processing').show();
                $('#pagenumber').val($(this).attr('pagenumber'));
                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });

            $('#productAdmin_cmdSearch').unbind("click");
            $('#productAdmin_cmdSearch').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#productAdmin_searchtext').val());

                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });

            $('#productAdmin_cmdReset').unbind("click");
            $('#productAdmin_cmdReset').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val('');
                $("#searchcategory").val('');

                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });

        }


        if (e.cmd == 'product_admin_getdetail') {
            $('.processing').hide();

        }

    };

  
});



