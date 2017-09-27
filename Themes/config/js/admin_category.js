$(document).ready(function() {

    $('.selectlang').unbind("click");
    $(".selectlang").click(function() {
        $('.actionbuttonwrapper').hide();
        $('.editlanguage').hide();
        $('.processing').show();
        $("#nextlang").val($(this).attr("editlang"));
        if ($("#razortemplate").val() == 'Admin_CategoryDetail.cshtml') {
            //move data to update postback field
            nbxget('product_admin_save', '#categorydatasection', '#actionreturn');
        } else {
            // Need to save list
            nbxget('product_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');
        }
    });

    $(document).on("nbxgetcompleted", Admin_category_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in product.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_CategoryList.cshtml');
    nbxget('category_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_category_nbxgetCompleted(e) {

        $('.actionbuttonwrapper').show();
        $('.editlanguage').show();

        setupbackoffice(); // run JS to deal with standard BO functions like accordian.


        //NBS - Tooltips
        $('[data-toggle="tooltip"]').tooltip({
            animation: 'true',
            placement: 'auto top',
            viewport: { selector: '#content', padding: 0 },
            delay: { show: 100, hide: 200 }
        });

        $('#productAdmin_cmdAddNew').unbind("click");
        $('#productAdmin_cmdAddNew').click(function () {
            $('.processing').show();
            $('#razortemplate').val('Admin_ProductDetail.cshtml');
            nbxget('product_adminaddnew', '#nbs_productadminsearch', '#datadisplay');
        });

        if (e.cmd == 'product_selectchangedisable' || e.cmd == 'product_selectchangehidden') {
            $('.processing').hide();
        };

        if (e.cmd == 'category_admin_getlist') {

            $('.processing').hide();

            $("#productAdmin_cmdSaveExit").hide();
            $("#productAdmin_cmdSave").hide();
            $("#productAdmin_cmdSaveAs").hide();
            $("#productAdmin_cmdDelete").hide();
            $("#productAdmin_cmdReturn").hide();
            $("#productAdmin_cmdAddNew").show();

            // Move products
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").show();
            $(".savebutton").hide();

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
            });

            $('.selectmove').unbind("click");
            $(".selectmove").click(function() {
                $(".selectmove").hide();
                $(".selectcancel").hide();
                $(".selectrecord").show();
                $("#movetoproductid").val($(this).attr("itemid"));
                nbxget('product_moveproductadmin', '#nbs_productadminsearch', '#datadisplay');
            });

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.categoryAdmin_cmdEdit').unbind("click");
            $('.categoryAdmin_cmdEdit').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_CategoryDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('category_admin_getdetail', '#nbs_categoryadminsearch', '#datadisplay');
            });


            $('.selectchangehidden').unbind("click");
            $('.selectchangehidden').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                if ($(this).hasClass("fa-check-circle")) {
                    $(this).addClass('fa-circle').removeClass('fa-check-circle');
                } else {
                    $(this).addClass('fa-check-circle').removeClass('fa-circle');
                }
                nbxget('product_selectchangehidden', '#nbs_productadminsearch');
            });

            $('.cmdopen').unbind("click");
            $(".cmdopen").click(function () {
                $('.processing').show();
                $('#returncatid').val($('#catid').val() + "," + $('#returncatid').val());
                $('#catid').val($(this).attr('itemid'));
                $('#razortemplate').val('Admin_CategoryList.cshtml');
                nbxget('category_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');
            });

            $('.cmdreturn').unbind("click");
            $(".cmdreturn").click(function () {
                $('.processing').show();
                var array = $('#returncatid').val().split(',');
                $('#catid').val(array[0]);
                $('#returncatid').val($('#returncatid').val().replace(array[0] + ",",""));
                $('#razortemplate').val('Admin_CategoryList.cshtml');
                nbxget('category_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');
            });

            $('#categoryAdmin_cmdAddNew').unbind("click");
            $("#categoryAdmin_cmdAddNew").click(function () {
                $('.processing').show();
                nbxget('category_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');
            });
            

        }

        if (e.cmd == 'category_admin_delete') {
            $('.processing').show();
            $('#razortemplate').val('Admin_CategoryList.cshtml');
            $('#selecteditemid').val('');
            nbxget('category_admin_getlist', '#nbs_categoryadminsearch', '#datadisplay');
        }

        if (e.cmd == 'product_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('product_admin_getdetail', '#nbs_productadminsearch', '#datadisplay');
        };

        if (e.cmd == 'product_admin_saveexit' || e.cmd == 'product_admin_saveas') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            $('#razortemplate').val('Admin_ProductList.cshtml');
            $('#selecteditemid').val('');
            nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
        };

        if (e.cmd == 'product_moveproductadmin') {
            $('#razortemplate').val('Admin_ProductList.cshtml');
            $('#selecteditemid').val('');
            nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
        };
        
        if (e.cmd == 'product_getproductselectlist') {
            $('.processing').hide();
        };

        if (e.cmd == 'product_admin_getdetail'
            || e.cmd == 'product_addproductmodels'
            || e.cmd == 'product_addproductoptions'
            || e.cmd == 'product_addproductoptionvalues'
            || e.cmd == 'product_updateproductdocs'
            || e.cmd == 'product_addproductcategory'
            || e.cmd == 'product_setdefaultcategory'
            || e.cmd == 'product_removeproductcategory'
            || e.cmd == 'product_populatecategorylist'
            || e.cmd == 'product_removeproperty'
            || e.cmd == 'product_addproperty'
            || e.cmd == 'product_addrelated'
            || e.cmd == 'product_removerelated'
            || e.cmd == 'product_adminaddnew'
            || e.cmd == 'product_updateproductimages') {

            // Copy the productid into the selecteditemid (for Add New Product)
            $('#selecteditemid').val($('#itemid').val());

            $('.actionbuttonwrapper').show();

            $('.processing').hide();

            $('.productsearchpanel').hide();
            
            $("#productAdmin_cmdSaveExit").show();
            $("#productAdmin_cmdSave").show();
            $("#productAdmin_cmdSaveAs").show();
            $("#productAdmin_cmdDelete").show();
            $("#productAdmin_cmdReturn").show();
            $("#productAdmin_cmdAddNew").hide();

            $('#datadisplay').children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
            $('#datadisplay').children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });


            // ---------------------------------------------------------------------------
            // FILE UPLOAD
            // ---------------------------------------------------------------------------
            var filecount = 0;
            var filesdone = 0;
            $(function () {
                'use strict';
                var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
                $('#fileupload').unbind('fileupload');
                $('#fileupload').fileupload({
                    url: url,
                    maxFileSize: 5000000,
                    acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i,
                    dataType: 'json'
                }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                    .bind('fileuploadprogressall', function (e, data) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        $('#progress .progress-bar').css('width', progress + '%');
                    })
                    .bind('fileuploadadd', function (e, data) {
                        $.each(data.files, function (index, file) {
                            $('input[id*="imguploadlist"]').val($('input[id*="imguploadlist"]').val() + file.name + ',');
                            filesdone = filesdone + 1;
                        });
                    })
                    .bind('fileuploadchange', function (e, data) {
                        filecount = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploaddrop', function (e, data) {
                        filecount = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploadstop', function (e) {
                        if (filesdone == filecount) {
                            nbxget('product_updateproductimages', '#nbs_productadminsearch', '#productimages'); // load images
                            filesdone = 0;
                            $('input[id*="imguploadlist"]').val('');
                            $('.processing').hide();
                            $('#progress .progress-bar').css('width', '0');
                        }
                    });
            });
            var filecount2 = 0;
            var filesdone2 = 0;
            $(function () {
                'use strict';
                var url = '/DesktopModules/NBright/NBrightBuy/XmlConnector.ashx?cmd=fileupload';
                $('#cmdSaveDoc').unbind();
                $('#cmdSaveDoc').fileupload({
                    url: url,
                    maxFileSize: 5000000,
                    acceptFileTypes: /(\.|\/)(png)$/i,
                    dataType: 'json'
                }).prop('disabled', !$.support.fileInput).parent().addClass($.support.fileInput ? undefined : 'disabled')
                    .bind('fileuploadprogressall', function (e, data) {
                        var progress = parseInt(data.loaded / data.total * 100, 10);
                        $('#progress .progress-bar').css('width', progress + '%');
                    })
                    .bind('fileuploadadd', function (e, data) {
                        $.each(data.files, function (index, file) {
                            $('input[id*="docuploadlist"]').val($('input[id*="docuploadlist"]').val() + file.name + ',');
                            filesdone2 = filesdone2 + 1;
                        });
                    })
                    .bind('fileuploadchange', function (e, data) {
                        filecount2 = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploaddrop', function (e, data) {
                        filecount2 = data.files.length;
                        $('.processing').show();
                    })
                    .bind('fileuploadstop', function (e) {
                        if (filesdone2 == filecount2) {
                            nbxget('product_updateproductdocs', '#nbs_productadminsearch', '#productdocs'); // load images
                            filesdone2 = 0;
                            $('input[id*="docuploadlist"]').val('');
                            $('.processing').hide();
                            $('#progress .progress-bar').css('width', '0');
                        }
                    });
            });


            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#productAdmin_cmdReturn').unbind("click");
            $('#productAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_ProductList.cshtml');
                $('#selecteditemid').val('');
                nbxget('product_admin_getlist', '#nbs_productadminsearch', '#datadisplay');
            });
            
            $('#productAdmin_cmdSave').unbind("click");
            $('#productAdmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                nbxget('product_admin_save', '#productdatasection', '#actionreturn');
            });

            $('#productAdmin_cmdSaveExit').unbind("click");
            $('#productAdmin_cmdSaveExit').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                nbxget('product_admin_saveexit', '#productdatasection', '#actionreturn');
            });

            $('#productAdmin_cmdSaveAs').unbind("click");
            $('#productAdmin_cmdSaveAs').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#productmodels', '.modelitem'));
                $('#xmlupdateoptiondata').val($.fn.genxmlajaxitems('#productoptions', '.optionitem'));
                $('#xmlupdateoptionvaluesdata').val($.fn.genxmlajaxitems('#productoptionvalues', '.optionvalueitem'));
                $('#xmlupdateproductimages').val($.fn.genxmlajaxitems('#productimages', '.imageitem'));
                $('#xmlupdateproductdocs').val($.fn.genxmlajaxitems('#productdocs', '.docitem'));
                nbxget('product_admin_saveas', '#productdatasection', '#actionreturn');
            });


            $('#productAdmin_cmdDelete').unbind("click");
            $('#productAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.actionbuttonwrapper').hide();
                    $('.editlanguage').hide();
                    $('.processing').show();
                    nbxget('product_admin_delete', '#nbs_productadminsearch', '#actionreturn');
                }
            });



            // ---------------------------------------------------------------------------
            // IMAGES
            // ---------------------------------------------------------------------------

            $('.removeimage').unbind("click");
            $('.removeimage').click(function () {
                removeelement($(this).parent().parent().parent().parent());
            });

            $('#undoimage').unbind("click");
            $('#undoimage').click(function () {
                 undoremove('.imageitem', '#productimages');
            });

            // ---------------------------------------------------------------------------
            // DOCS
            // ---------------------------------------------------------------------------

            $('.removedoc').unbind();
            $('.removedoc').click(function() {
                 removeelement($(this).parent().parent().parent().parent());
            });

            $('#undodoc').unbind();
            $('#undodoc').click(function () {
                 undoremove('.docitem', '#productdocs');
            });




          
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

