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
            nbxget('productoptionvalues', '#productselectparams', 'productoptionvalues', '#productoptionvalues'); // load options
        });
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
    $('a[id*="cmdSave"]').click(function () {
        $('.processing').show();
        $('input[id*="updatemodeldata"]').val($.fn.genxmlajaxitems('#productmodels', '.modelitem')); //move model data to update postback field
        $('input[id*="updateproductoptions"]').val($.fn.genxmlajaxitems('#productoptions', '.optionitem')); //move model data to update postback field
        $('input[id*="updateproductoptionvalues"]').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem')); //move model data to update postback field
        $('input[id*="updateproductimages"]').val($.fn.genxmlajaxitems('#productimages', '.imageitem')); //move model data to update postback field
        $('input[id*="updateproductdocs"]').val($.fn.genxmlajaxitems('#productdocs', '.docitem')); //move model data to update postback field
    });

});
