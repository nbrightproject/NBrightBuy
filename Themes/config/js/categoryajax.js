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
            nbxget('getcategoryproductlist', '#categorydata', 'getcategoryproductlist', '#productlist');
        });
        // select product
        $('#productselect').click(function () {
            $("input[id*='header']").val("productselectheader.html");
            $("input[id*='body']").val("productselectbody.html");
            $("input[id*='footer']").val("productselectfooter.html");
            nbxget('getproductselectlist', '#productselectparams', 'getproductselectlist', '#productselectlist');
            $('#categorydatasection').hide();
            $('#productselectsection').show();
        });
        // remove single product
        $('.removeproduct').click(function () {
            nbxget('deletecatxref', '.productid' + $(this).attr('itemid'), 'deletecatxref', '#nbsnotify');
            $('.productid' + $(this).attr('itemid')).hide();
        });
        $('#removeall').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('deleteallcatxref', '#productselectparams', 'deleteallcatxref', '#nbsnotify');
            }
        });
        $('#copyto').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('copyallcatxref', '#productselectparams', 'copyallcatxref', '#nbsnotify');
            }
        });
        $('#moveto').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('moveallcatxref', '#productselectparams', 'moveallcatxref', '#nbsnotify');
            }
        });

        $('select[id*="selectcatid"]').change(function () {
            $('input[id*="selectedcatid"]').val($(this).val());
        });
    });

    $('#productselectlist').change(function () {
        //Do paging
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getproductselectlist', '#productselectparams', 'getproductselectlist', '#productselectlist');
        });
        // return from select product
        $('#returnfromselect').click(function () {
            $("input[id*='header']").val("categoryproductheader.html");
            $("input[id*='body']").val("categoryproductbody.html");
            $("input[id*='footer']").val("categoryproductfooter.html");
            $("input[id*='searchtext']").val('');
            $("input[id*='searchcategory']").val('');
            nbxget('getcategoryproductlist', '#categorydata', 'getcategoryproductlist', '#productlist');
            $('#productselectsection').hide();
            $('#categorydatasection').show();
        });
        // select product
        $('.selectproduct').click(function () {
            nbxget('selectcatxref', '.selproductid' + $(this).attr('itemid'), 'selectcatxref');
            $('.selectproductid' + $(this).attr('itemid')).hide();
        });

    });


    // START: --------   Search products ----------------------
    // select search
    $('#selectsearch').click(function () {
        $('input[id*="searchtext"]').val($('input[id*="txtSearch"]').val());
        $('input[id*="searchcategory"]').val($('select[id*="ddlsearchcategory"]').val());
        nbxget('getproductselectlist', '#productselectparams', 'getproductselectlist', '#productselectlist');
    });

    // select search reset
    $('#selectreset').click(function () {
        $('input[id*="txtSearch"]').val('');
        $('select[id*="ddlsearchcategory"]').val('');
        $('input[id*="searchtext"]').val('');
        $('input[id*="searchcategory"]').val('');
        nbxget('getproductselectlist', '#productselectparams', 'getproductselectlist', '#productselectlist');
    });
    // END: -------------------------------------------------------

    //show processing on postback for image.
    $("body").on("click", ".postbacklink", function () {
        $('.processing').show();
    });


    nbxget('getcategoryproductlist', '#categorydata', 'getcategoryproductlist', '#productlist');

});