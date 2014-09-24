$(document).ready(function () {

    var showprocessing = true;

    $(document).bind("ajaxSend", function () {
        $('#nbsnotify').html(''); // clear message on each load.          
        if (showprocessing) $('.processing').show();
    }).bind("ajaxComplete", function () {
        $("#loading").hide();
        $('.processing').hide();
        showprocessing = true;
    });

    $('#productlist').change(function () {
        //Do paging
        $('.cmdPg').unbind();
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getcategoryproductlist', '#categorydata', '#productlist');
        });
        // select product
        $('#productselect').unbind();
        $('#productselect').click(function () {
            $("input[id*='header']").val("productselectheader.html");
            $("input[id*='body']").val("productselectbody.html");
            $("input[id*='footer']").val("productselectfooter.html");
            nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
            $('#categorydatasection').hide();
            $('.actionbuttonwrapper').hide();
            $('#productselectsection').show();
        });
        // remove single product
        $('.removeproduct').unbind();
        $('.removeproduct').click(function () {
            showprocessing = false;
            nbxget('deletecatxref', '.productid' + $(this).attr('itemid'), '#nbsnotify');
            //$('.productid' + $(this).attr('itemid')).hide();
            nbxget('getcategoryproductlist', '#categorydata', '#productlist');
        });
        $('#removeall').unbind();
        $('#removeall').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('deleteallcatxref', '#productselectparams', '#nbsnotify');
            }
        });
        $('#copyto').unbind();
        $('#copyto').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('copyallcatxref', '#productselectparams', '#nbsnotify');
            }
        });
        $('#moveto').unbind();
        $('#moveto').click(function () {
            if (confirm($('#confirmmsg').html())) {
                nbxget('moveallcatxref', '#productselectparams', '#nbsnotify');
            }
        });

        $('select[id*="selectcatid"]').change(function () {
            $('input[id*="selectedcatid"]').val($(this).val());
        });
    });

    $('#productselectlist').change(function () {
        //Do paging
        $('.cmdPg').unbind();
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
        });
        // return from select product
        $('#returnfromselect').unbind();
        $('#returnfromselect').click(function () {
            $("input[id*='header']").val("categoryproductheader.html");
            $("input[id*='body']").val("categoryproductbody.html");
            $("input[id*='footer']").val("categoryproductfooter.html");
            $("input[id*='searchtext']").val('');
            $("input[id*='searchcategory']").val('');
            nbxget('getcategoryproductlist', '#categorydata', '#productlist');
            $('#productselectsection').hide();
            $('#categorydatasection').show();
            $('.actionbuttonwrapper').show();
        });
        // select product
        $('.selectproduct').unbind();
        $('.selectproduct').click(function () {
            showprocessing = false;
            nbxget('selectcatxref', '.selproductid' + $(this).attr('itemid'));
            $('.selectproductid' + $(this).attr('itemid')).hide();
        });

    });


    // START: --------   Search products ----------------------
    // select search
    $('#selectsearch').click(function () {
        $('input[id*="searchtext"]').val($('input[id*="txtSearch"]').val());
        $('input[id*="searchcategory"]').val($('select[id*="ddlsearchcategory"]').val());
        nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
    });

    // select search reset
    $('#selectreset').click(function () {
        $('input[id*="txtSearch"]').val('');
        $('select[id*="ddlsearchcategory"]').val('');
        $('input[id*="searchtext"]').val('');
        $('input[id*="searchcategory"]').val('');
        nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
    });
    // END: -------------------------------------------------------

    //show processing on postback for image.
    $("body").on("click", ".postbacklink", function () {
        $('.processing').show();
    });

    showprocessing = false;
    nbxget('getcategoryproductlist', '#categorydata', '#productlist');

});