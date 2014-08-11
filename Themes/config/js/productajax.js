$(document).ready(function () {

    $('input[id*="nbxprocessing"]').change(function () {
        if ($(this).val() == 'BEFORE') {
            $('#nbsnotify').html(''); // clear message on each load.          
            $('.processing').show();
        }
        if ($(this).val() == 'AFTER') $('.processing').hide();
    });

    $('#productlist').change(function () {
        //-------------------------------------------------------
        // ---------------  Product Select ----------------------
        //-------------------------------------------------------
        if ($('input[id*="nbxaction"]').val() == 'getproductlist') {
            //Do paging
            $('.cmdPg').click(function () {
                $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
                nbxget('getproductselectlist', '#productselectparams', 'getproductlist', '#productlist');
            });
            // select product
            $('.selectproduct').click(function () {
                $('input[id*="itemid"]').val($(this).attr('itemid'));
                nbxget('editproduct', '#productselectparams', 'editproduct', '#productgeneral');
                $('#productsearchsection').hide();
                $('#productlistsection').hide();
                $('#productdatasection').show();
            });
        }
    });

    $('#productgeneral').change(function () {
        nbxget('productdescription', '#productselectparams', 'productdescription', 'textarea[id*="description"]'); // load description data into ckeditor
        nbxget('productmodels', '#productselectparams', 'productmodels', '#productmodels'); // load models
        nbxget('productoptions', '#productselectparams', 'productoptions', '#productoptions'); // load options
        nbxget('productimages', '#productselectparams', 'productimages', '#productimages'); // load images
        nbxget('productdocs', '#productselectparams', 'productdocs', '#productdocs'); // load docs
        nbxget('productcategories', '#productselectparams', 'productcategories', '#productcategories'); // load docs
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
        nbxget('getproductlist', '#productselectparams', 'getproductlist');
    });

    // select search reset
    $('#listreset').click(function () {
        $('input[id*="txtSearch"]').val('');
        $('select[id*="ddllistsearchcategory"]').val('');
        $('input[id*="searchtext"]').val('');
        $('input[id*="searchcategory"]').val('');
        nbxget('getproductlist', '#productselectparams', 'getproductlist');
    });
    // END: -------------------------------------------------------

    //show processing on postback for image.
    $("body").on("click", ".postbacklink", function () {
        $('.processing').show();
    });

    // save the product data
    $('#productsave').click(function () {
        CKEditor_TextBoxEncode($('textarea[id*="message"]').attr('id'), 0); // need this to get ckeditor text
        //nbxget('setproductadminform', '#productdata', 'setproduct');
    });

    $('#returntolist').click(function () {
        $('#productsearchsection').show();
        $('#productlistsection').show();
        $('#productdatasection').hide();
    });


    nbxget('getproductlist', '#productselectparams', 'getproductlist', '#productlist');

});
