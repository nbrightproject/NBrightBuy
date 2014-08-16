$(document).ready(function () {

    $('input[id*="nbxprocessing"]').change(function () {
        if ($(this).val() == 'BEFORE') {
            $('#nbsnotify').html(''); // clear message on each load.          
            $('.processing').show();
        }
        if ($(this).val() == 'AFTER') $('.processing').hide();
    });

    $('#productlist').change(function () {
        //Do paging
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getproductselectlist', '#productselectparams', 'getproductlist', '#productlist');
        });
    });

    $('#productoptions').change(function () {
        // select option
        $('.selectoption').click(function () {
            $('input[id*="selectedoptionid"]').val($(this).attr('itemid'));
            $(this).parent().parent().children().removeClass('selected');
            $(this).parent().addClass('selected');
            showoptionvalues();
        });
    });


    $('#productmodels').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removemodel').click(function () { removeelement($(this).parent()); });
    });

    $('#productoptions').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeoption').click(function () {
            removeelement($(this).parent());
            if ($(this).parent().hasClass('selected')) {
                $('#productoptionvalues').hide();
                $(this).parent().removeClass('selected');
            }
        });
        //trigger select option, to display correct option values
        $('.selectoption').first().trigger('click');
    });
    $('#productoptionvalues').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeoptionvalue').click(function () { removeelement($(this).parent()); });
        $('#optionvaluecontrol').show();
        showoptionvalues();
    });
    $('#productimages').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeimage').click(function () { removeelement($(this).parent()); });
    });
    $('#productdocs').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removedoc').click(function () { removeelement($(this).parent()); });
    });
    $('#productcategories').change(function () {
        $('.removedoc').click(function () { removeelement($(this).parent()); });
    });
    $('#productrelated').change(function () {
        $('.removedoc').click(function () { removeelement($(this).parent()); });
    });


    $('#undomodel').click(function () { undoremove('.modelitem', '#productmodels'); });
    $('#undooption').click(function () { undoremove('.optionitem', '#productoptions'); });
    $('#undooptionvalue').click(function () { undoremove('.optionvalueitem', '#productoptionvalues'); });
    $('#undoimage').click(function () { undoremove('.imageitem', '#productimages'); });
    $('#undodoc').click(function () { undoremove('.docitem', '#productdocs'); });
    $('#undocategory').click(function () { undoremove('.categoryitem', '#productcategories'); });
    $('#undorelated').click(function () { undoremove('.relateditem', '#productrelated'); });

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
            if (itemselector == '.categoryitem') $('#undocategory').hide(); dc
            if (itemselector == '.relateditem') $('#undorelated').hide();
        }
    }
    function showoptionvalues() {
        $('#productoptionvalues').children().hide();
        if ($('#productoptions').children('.selected').first().find('input[id*="optionid"]').length > 0) {
            $('#productoptionvalues').children('.' + $('#productoptions').children('.selected').first().find('input[id*="optionid"]').val()).show();
            $('#productoptionvalues').show();
        }
    }



    //-------------------------------------------------------
    // -------------   Search products ----------------------
    //-------------------------------------------------------
    // select search
    $('#listsearch').click(function () {
        $('input[id*="searchtext"]').val($('input[id*="txtSearch"]').val());
        $('input[id*="searchcategory"]').val($('select[id*="ddllistsearchcategory"]').val());
        nbxget('getproductlist', '#productselectparams', 'getproductlist', '#productlist');
    });

    // select search reset
    $('#listreset').click(function () {
        $('input[id*="txtSearch"]').val('');
        $('select[id*="ddllistsearchcategory"]').val('');
        $('input[id*="searchtext"]').val('');
        $('input[id*="searchcategory"]').val('');
        nbxget('getproductlist', '#productselectparams', 'getproductlist', '#productlist');
    });
    // END: -------------------------------------------------------

    //show processing on postback for image.
    $('a[id*="cmdReturn"]').click(function () {
        $('.processing').show();
    });
    // save form with postback
    $('a[id*="cmdSave"]').click(function () {
        $('.processing').show();
        //move data to update postback field   
        $('input[id*="xmlupdatemodeldata"]').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
        $('input[id*="xmlupdateproductoptions"]').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
        $('input[id*="xmlupdateproductoptionvalues"]').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
        $('input[id*="xmlupdateproductimages"]').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
        $('input[id*="xmlupdateproductdocs"]').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
        $('input[id*="xmlupdateproductcategories"]').val($.fn.genxmlajaxitems('#productcategories', '.categoryitem'));
        $('input[id*="xmlupdateproductrelated"]').val($.fn.genxmlajaxitems('#productrelated', '.relateditem'));
    });

    //Add models
    $('#addmodels').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddmodelqty"]').val());
        nbxget('addproductmodels', '#productselectparams', 'addproductmodels', '#productmodels'); // load models
    });
    //Add options
    $('#addopt').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddoptqty"]').val());
        nbxget('addproductoptions', '#productselectparams', 'addproductoptions', '#productoptions'); // load options
    });
    //Add optionvalues
    $('#addoptvalues').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddoptvalueqty"]').val());
        nbxget('addproductoptionvalues', '#productselectparams', 'addproductoptionvalues', '#productoptionvalues'); // load optionvalues
    });

});
