$(document).ready(function() {


    $('.selectlang').unbind("click");
    $(".selectlang").click(function() {
        $("#nextlang").val($(this).attr("editlang"));
        if ($("#razortemplate").val() == 'Admin_ProductsDetail.cshtml') {
            $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
            nbxget('product_admin_save', '#productdatasection', '#actionreturn');
        } else {
            nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
        }
    });


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

            $("#productAdmin_cmdSaveExit").hide();
            $("#productAdmin_cmdSave").hide();
            $("#productAdmin_cmdSaveAs").hide();
            $("#productAdmin_cmdDelete").hide();
            $("#productAdmin_cmdReturn").hide();

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

            $('#productAdmin_searchtext').val($('#searchtext').val());

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.productAdmin_cmdEdit').unbind("click");
            $('.productAdmin_cmdEdit').click(function() {
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

        if (e.cmd == 'product_admin_delete') {
            $('.processing').show();
            $('#razortemplate').val('Admin_ProductsList.cshtml');
            $('#selecteditemid').val('');
            nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
        }

        if (e.cmd == 'product_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('product_admin_getdetail', '#nbs_productadminsearch', '#datadisplay');
        };

        if (e.cmd == 'product_admin_getdetail' || e.cmd == 'product_addproductmodels' || e.cmd == 'product_addproductoptions') {
            $('.processing').hide();

            $("#productAdmin_cmdSaveExit").show();
            $("#productAdmin_cmdSave").show();
            $("#productAdmin_cmdSaveAs").show();
            $("#productAdmin_cmdDelete").show();
            $("#productAdmin_cmdReturn").show();

            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#productAdmin_cmdReturn').unbind("click");
            $('#productAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductsList.cshtml');
                $('#selecteditemid').val('');
                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });

            $('#productAdmin_cmdSave').unbind("click");
            $('#productAdmin_cmdSave').click(function () {
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                nbxget('product_admin_save', '#productdatasection', '#actionreturn');
            });

            $('#productAdmin_cmdDelete').unbind("click");
            $('#productAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.processing').show();
                    nbxget('product_admin_delete', '#nbs_productadminsearch', '#actionreturn');
                }
            });



            // ---------------------------------------------------------------------------
            // STOCK CONTROL
            // ---------------------------------------------------------------------------
            $('.chkstockon:not(:checked)').each(function (index) {
                $(this).parent().parent().next().hide();
            });

            $('.selectrecord').unbind("click");
            $('.chkstockon').click(function () {
                if ($(this).is(":checked")) {
                    $(this).parent().parent().next().show();
                } else {
                    $(this).parent().parent().next().hide();
                }
            });


            // ---------------------------------------------------------------------------
            // MODELS
            // ---------------------------------------------------------------------------
            $('.removemodel').unbind("click");
            $('.removemodel').click(function () { removeelement($(this).parent().parent().parent().parent()); });
            $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
            $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
            $('input[id*="availabledate"]').datepicker();

            //Add models
            $('#addmodels').unbind("click");
            $('#addmodels').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddmodelqty').val());
                nbxget('product_addproductmodels', '#nbs_productadminsearch', '#datadisplay'); // load models
            });

            $('#undomodel').unbind("click");
            $('#undomodel').click(function() {
                 undoremove('.modelitem', '#productmodels');
            });
            $('.chkdisabledealer:checked').each(function (index) {
                $(this).prev().attr("disabled", "disabled");;
                $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
            });
            $('.chkdisabledealer').unbind("change");
            $('.chkdisabledealer').change(function () {
                if ($(this).is(":checked")) {
                    $(this).prev().attr("disabled", "disabled");
                    $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
                    $(this).prev().val(0);
                    $(this).parent().next().find('.dealersale').val(0);
                } else {
                    $(this).prev().removeAttr("disabled");
                    $(this).parent().next().find('.dealersale').removeAttr("disabled");
                }
            });

            $('.chkdisablesale:checked').each(function (index) {
                $(this).prev().attr("disabled", "disabled");;
            });
            $('.chkdisablesale').unbind("change");
            $('.chkdisablesale').change(function () {
                if ($(this).is(":checked")) {
                    $(this).prev().attr("disabled", "disabled");;
                    $(this).prev().val(0);
                } else {
                    $(this).prev().removeAttr("disabled");
                }
            });

            // ---------------------------------------------------------------------------
            // OPTIONS
            // ---------------------------------------------------------------------------

            //Add options
            $('#addopt').unbind("click");
            $('#addopt').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddoptqty').val());
                nbxget('product_addproductoptions', '#nbs_productadminsearch', '#datadisplay'); 
            });

            $('#undooption').unbind("click");
            $('#undooption').click(function () {
                undoremove('.optionitem', '#productoptions');
            });

            $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
            $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });

            $('.removeoption').click(function () {
                removeelement($(this).parent().parent().parent().parent());
                if ($(this).parent().parent().parent().parent().hasClass('selected')) {
                    $('#productoptionvalues').hide();
                    $(this).parent().parent().parent().parent().removeClass('selected');
                }
            });

            $('.selectoption').click(function () {
                $('#selectedoptionid').val($(this).attr('itemid'));
                $(this).parent().parent().parent().parent().parent().children().removeClass('selected');
                $(this).parent().parent().parent().parent().addClass('selected');
                showoptionvalues();
            });

            //Add optionvalues
            $('#addoptvalues').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddoptvalueqty').val());
                nbxget('product_addproductoptionvalues', '#nbs_productadminsearch', '#productoptionvalues');
            });

            $('.removeoptionvalue').click(function() {
                 removeelement($(this).parent().parent().parent().parent());
            });

            $('#undooptionvalue').click(function() {
                 undoremove('.optionvalueitem', '#productoptionvalues');
            });

            //trigger select option, to display correct option values
            $('.selectoption').last().trigger('click');


        }

    };

    // ---------------------------------------------------------------------------
    // FUNCTIONS
    // ---------------------------------------------------------------------------
    function moveUp(item) {
        var prev = item.prev();
        if (prev.length == 0)
            return;
        prev.css('z-index', 999).css('position', 'relative').animate({ top: item.height() }, 250);
        item.css('z-index', 1000).css('position', 'relative').animate({ top: '-' + prev.height() }, 300, function () {
            prev.css('z-index', '').css('top', '').css('position', '');
            item.css('z-index', '').css('top', '').css('position', '');
            item.insertBefore(prev);
        });
    }
    function moveDown(item) {
        var next = item.next();
        if (next.length == 0)
            return;
        next.css('z-index', 999).css('position', 'relative').animate({ top: '-' + item.height() }, 250);
        item.css('z-index', 1000).css('position', 'relative').animate({ top: next.height() }, 300, function () {
            next.css('z-index', '').css('top', '').css('position', '');
            item.css('z-index', '').css('top', '').css('position', '');
            item.insertAfter(next);
        });
    }

    function removeelement(elementtoberemoved) {
        if ($('#recyclebin').length > 0) {
            $('#recyclebin').append($(elementtoberemoved));
        } else { $(elementtoberemoved).remove(); }
        if ($(elementtoberemoved).hasClass('modelitem')) $('#undomodel').show();
        if ($(elementtoberemoved).hasClass('optionitem')) $('#undooption').show();
        if ($(elementtoberemoved).hasClass('optionvalueitem')) $('#undooptionvalue').show();
        if ($(elementtoberemoved).hasClass('imageitem')) $('#undoimage').show();
        if ($(elementtoberemoved).hasClass('docitem')) $('#undodoc').show();
        if ($(elementtoberemoved).hasClass('categoryitem')) $('#undocategory').show();
        if ($(elementtoberemoved).hasClass('relateditem')) $('#undorelated').show();
        if ($(elementtoberemoved).hasClass('clientitem')) $('#undoclient').show();
    }
    function undoremove(itemselector, destinationselector) {
        if ($('#recyclebin').length > 0) {
            $(destinationselector).append($('#recyclebin').find(itemselector).last());
        }
        if ($('#recyclebin').children(itemselector).length == 0) {
            if (itemselector == '.modelitem') $('#undomodel').hide();
            if (itemselector == '.optionitem') $('#undooption').hide();
            if (itemselector == '.optionvalueitem') $('#undooptionvalue').hide();
            if (itemselector == '.imageitem') $('#undoimage').hide();
            if (itemselector == '.docitem') $('#undodoc').hide();
            if (itemselector == '.categoryitem') $('#undocategory').hide();
            if (itemselector == '.relateditem') $('#undorelated').hide();
            if (itemselector == '.clientitem') $('#undoclient').hide();
        }
    }

    function showoptionvalues() {
        $('#productoptionvalues').children().hide();
        if ($('#productoptions').children('.selected').first().find('input[id*="optionid"]').length > 0) {
            $('#productoptionvalues').children('.' + $('#productoptions').children('.selected').first().find('input[id*="optionid"]').val()).show();
            $('#productoptionvalues').show();
        }
        $('#optionvaluecontrol').show();
    }


    // ---------------------------------------------------------------------------

});



