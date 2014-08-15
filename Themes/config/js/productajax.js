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
            nbxget('updateproductoptionvalues', '#productoptionvalues', 'updateproductoptionvalues', '#notifymsg', '.optionvalueitem'); // update optionvalues
            $('input[id*="selectedoptionid"]').val($(this).attr('itemid'));
            nbxget('productoptionvalues', '#productselectparams', 'productoptionvalues', '#productoptionvalues'); // load optionvalues
        });
    });


    $('#productmodels').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removemodel').click(function () { $(this).parent().remove(); });
    });

    $('#productoptions').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeoption').click(function () { $(this).parent().remove(); });
    });
    $('#productoptionvalues').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeoptionvalue').click(function () { $(this).parent().remove(); });
        $('#optionvaluecontrol').show();
    });
    $('#productimages').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removeimage').click(function () { $(this).parent().remove(); });
    });
    $('#productdocs').change(function () {
        $(this).sortable();
        $(this).disableSelection();
        $('.removedoc').click(function () { $(this).parent().remove(); });
    });



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
