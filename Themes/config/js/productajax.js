$(document).ready(function () {

    $(document).bind("ajaxSend", function () {
        $('#nbsnotify').html(''); // clear message on each load.          
        $('.processing').show();
    }).bind("ajaxComplete", function () {
        $("#loading").hide();
        $('.processing').hide();
    });



    $('#productlist').change(function () {
        //Do paging
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getproductselectlist', '#productselectparams', '#productlist');
        });
    });

    $('#productoptions').change(function () {
        // select option
        $('.selectoption').click(function () {
            $('input[id*="selectedoptionid"]').val($(this).attr('itemid'));
            $(this).parent().parent().parent().parent().parent().children().removeClass('selected');
            $(this).parent().parent().parent().parent().addClass('selected');
            showoptionvalues();
        });
    });


    $('#productmodels').change(function () {
        $('.removemodel').click(function () { removeelement($(this).parent().parent().parent().parent()); });
        $(this).children().find('.sortelementUp').click(function () {moveUp($(this).parent()); });
        $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
        if (!Modernizr.inputtypes.date) $('input[id*="availabledate"]').datepicker();


        $('.activatestock input[id*="_chkstockon_"]:not(:checked)').each(function( index ) {
            $(this).parent().parent().next().hide();
        });

        $('.activatestock input[id*="_chkstockon_"]').click(function() {
            if($(this).is(":checked")) {
                $(this).parent().parent().next().show();
            } else {
                $(this).parent().parent().next().hide();
            }
        });

        $('input[id*="_chkdisabledealer_"]:checked').each(function( index ) {
                $(this).prev().attr("disabled", "disabled");;            
                $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
        });
        $('input[id*="_chkdisabledealer_"]').change(function() {
            if($(this).is(":checked")) {
                $(this).prev().attr("disabled", "disabled");
                $(this).parent().next().find('.dealersale').attr("disabled", "disabled");
                $(this).prev().val(0);
                $(this).parent().next().find('.dealersale').val(0);
            } else {
                $(this).prev().removeAttr("disabled");
                $(this).parent().next().find('.dealersale').removeAttr("disabled");      
            }        
        });

        $('input[id*="_chkdisablesale_"]:checked').each(function( index ) {
                $(this).prev().attr("disabled", "disabled");;            
        });
        $('input[id*="_chkdisablesale_"]').change(function() {
            if($(this).is(":checked")) {
                $(this).prev().attr("disabled", "disabled");;
                $(this).prev().val(0);
            } else {
                $(this).prev().removeAttr("disabled");
            }        
        });

    });

    $('#productoptions').change(function () {
        $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
        $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
        $('.removeoption').click(function () {
            removeelement($(this).parent().parent().parent().parent());
            if ($(this).parent().parent().parent().parent().hasClass('selected')) {
                $('#productoptionvalues').hide();
                $(this).parent().parent().parent().parent().removeClass('selected');
            }
        });
        //trigger select option, to display correct option values
        $('.selectoption').last().trigger('click');
    });
    $('#productoptionvalues').change(function () {
        $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
        $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
        $('.removeoptionvalue').click(function () { removeelement($(this).parent().parent().parent().parent()); });
        $('#optionvaluecontrol').show();
        showoptionvalues();
    });
    $('#productimages').change(function () {
        $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
        $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
        $('.removeimage').click(function () { removeelement($(this).parent().parent().parent().parent()); });

        // ajax upload actions
        $('.processing').hide();
        $('#progress .progress-bar').css('width', '0');

    });
    $('#productdocs').change(function () {
        $(this).children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
        $(this).children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });
        $('.removedoc').unbind();
        $('.removedoc').click(function () { removeelement($(this).parent().parent().parent().parent()); });
    });
    $('#productcategories').change(function () {
        $('.removecategory').unbind();
        $('.removecategory').click(function () {
            $('input[id*="selectedcatid"]').val($(this).attr('categoryid'));
            nbxget('removeproductcategory', '#productselectparams', '#productcategories'); // load             
        });
        // set default category
        $('.defaultcategory').click(function () {
            $('input[id*="selectedcatid"]').val($(this).attr('categoryid'));
            nbxget('setdefaultcategory', '#productselectparams', '#productcategories'); // load             
        });

    });

    $('#groupcategorylist').change(function () {
        // select group category
        $('.selectgroupcategory').unbind();  //remove event so we don't get duplicate on the element click.
        $('.selectgroupcategory').click(function () {
            $('input[id*="selectedcatid"]').val($(this).val());
            nbxget('addproductgroupcategory', '#productselectparams', '#productgroupcategories'); // load 
        });
    });

    $('#productgroupcategories').change(function () {
        $('.removegroupcategory').unbind();
        $('.removegroupcategory').click(function () {
            $('input[id*="selectedcatid"]').val($(this).attr('categoryid'));
            nbxget('removeproductgroupcategory', '#productselectparams', '#productgroupcategories'); // load             
        });
    });


    $('#productrelated').change(function () {
        $('.removerelated').click(function () {
            $('input[id*="selectedrelatedid"]').val($(this).attr('productid'));
            nbxget('removerelatedproduct', '#productselectparams', '#productrelated'); // load releated
        });
    });


    $('#undomodel').click(function () { undoremove('.modelitem', '#productmodels'); });
    $('#undooption').click(function () { undoremove('.optionitem', '#productoptions'); });
    $('#undooptionvalue').click(function () { undoremove('.optionvalueitem', '#productoptionvalues'); });
    $('#undoimage').click(function () { undoremove('.imageitem', '#productimages'); });
    $('#undodoc').click(function () { undoremove('.docitem', '#productdocs'); });
    $('#undorelated').click(function () { undoremove('.relateditem', '#productrelated'); });
    $('#undoclient').click(function () { undoremove('.clientitem', '#productclients'); });

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
    }



    //-------------------------------------------------------
    // -------------   Search LIST products ----------------------
    //-------------------------------------------------------
    // select search
    $('#listsearch').click(function () {
        $('input[id*="searchtext"]').val($('input[id*="txtSearch"]').val());
        $('input[id*="searchcategory"]').val($('select[id*="ddllistsearchcategory"]').val());
        nbxget('getproductlist', '#productselectparams', '#productlist');
    });

    // select search reset
    $('#listreset').click(function () {
        $('input[id*="txtSearch"]').val('');
        $('select[id*="ddllistsearchcategory"]').val('');
        $('input[id*="searchtext"]').val('');
        $('input[id*="searchcategory"]').val('');
        nbxget('getproductlist', '#productselectparams', '#productlist');
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
        $('input[id*="xmlupdateproductclients"]').val($.fn.genxmlajaxitems('#productclients', '.clientitem'));
    });

    //Add models
    $('#addmodels').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddmodelqty"]').val());
        nbxget('addproductmodels', '#productselectparams', '#productmodels', null, 'true'); // load models
    });
    //Add options
    $('#addopt').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddoptqty"]').val());
        nbxget('addproductoptions', '#productselectparams', '#productoptions', null, 'true'); // load options
    });
    //Add optionvalues
    $('#addoptvalues').click(function () {
        $('.processing').show();
        $('input[id*="addqty"]').val($('input[id*="txtaddoptvalueqty"]').val());
        nbxget('addproductoptionvalues', '#productselectparams', '#productoptionvalues', null, 'true'); // load optionvalues
    });

    //Add category
    $('select[id*="selectcategory"]').click(function () {
        $('input[id*="selectedcatid"]').val($(this).val());
        if ($(this).val() != null) nbxget('addproductcategory', '#productselectparams', '#productcategories'); // load 
    });
    
    //select group category
    $('select[id*="selectgrouptype"]').click(function () {
        $('input[id*="selectedgroupref"]').val($(this).val());
        if ($(this).val() != null) nbxget('populatecategorylist', '#productselectparams', '#groupcategorylist'); // load 
    });

    $('#productselect').click(function () {
        $(this).hide();
        $("input[id*='header']").val("productselectheader.html");
        $("input[id*='body']").val("productselectbody.html");
        $("input[id*='footer']").val("productselectfooter.html");
        nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
        $('#productdatasection').hide();
        $('#productselectsection').show();
    });

    $('#returnfromselect').click(function () {
        $('#productselect').show();
        $("input[id*='header']").val("productlistheader.html");
        $("input[id*='body']").val("productlistbody.html");
        $("input[id*='footer']").val("productlistfooter.html");
        $("input[id*='searchtext']").val('');
        $("input[id*='searchcategory']").val('');
        nbxget('getcategoryproductlist', '#categorydata', '#productlist');
        $('#productselectsection').hide();
        $('#productdatasection').show();
    });



    //--------------------------------------------------------------------------------
    //----- related product select ---------------------------------------------------
    //--------------------------------------------------------------------------------

    $('#productselectlist').change(function () {
        //Do paging
        $('.cmdPg').unbind();
        $('.cmdPg').click(function () {
            $('input[id*="pagenumber"]').val($(this).attr("pagenumber"));
            nbxget('getproductselectlist', '#productselectparams', '#productselectlist');
        });
        // select product
        $('.selectproduct').unbind();
        $('.selectproduct').click(function () {
            $('.selectproductid' + $(this).attr('itemid')).hide();
            $('input[id*="selectedrelatedid"]').val($(this).attr('itemid'));
            nbxget('addrelatedproduct', '#productselectparams', '#productrelated'); // load releated
        });

    });


    // START: --------   Search SELECT products ----------------------
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


    //-------------------------------------------------------
    // -------------  CLIENT SELECT    ----------------------
    //-------------------------------------------------------

    $('#clientselectlist').change(function () {
        // select product
        $('.selectclient').unbind();
        $('.selectclient').click(function () {
            $('.selectuserid' + $(this).attr('itemid')).hide();
            $('input[id*="selecteduserid"]').val($(this).attr('itemid'));
            nbxget('addproductclient', '#productselectparams', '#productclients'); // load releated
        });
    });


    // select search
    $('#clientlistsearch').click(function () {
        $('input[id*="searchtext"]').val($('input[id*="txtClientSearch"]').val());
        nbxget('getclientselectlist', '#productselectparams', '#clientselectlist');
    });

    $('#clientselect').click(function () {
        $(this).hide();
        $("input[id*='header']").val("clientselectheader.html");
        $("input[id*='body']").val("clientselectbody.html");
        $("input[id*='footer']").val("clientselectfooter.html");
        $('#productdatasection').hide();
        $('#clientselectsection').show();
    });

    $('#returnfromclientselect').click(function () {
        $('#clientselect').show();
        $("input[id*='header']").val("productlistheader.html");
        $("input[id*='body']").val("productlistbody.html");
        $("input[id*='footer']").val("productlistfooter.html");
        $("input[id*='searchtext']").val('');
        nbxget('productclients', '#productdata', '#productclients');
        $('#clientselectsection').hide();
        $('#productdatasection').show();
    });

    $('#productclients').change(function () {
        $('.removeclient').click(function () {
            $('input[id*="selecteduserid"]').val($(this).attr('itemid'));
            nbxget('removeproductclient', '#productselectparams', '#productclients');
        });
    });

    //-------------------------------------------------------
    // CLIENT SELECT END: -----------------------------------
    //-------------------------------------------------------



    $('#productselect').click(function () {
        $('#pageactionssource').toggle();
    });
    $('#returnfromselect').click(function () {
        $('#pageactionssource').toggle();
    });

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

});



