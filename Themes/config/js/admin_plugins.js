$(document).ready(function() {

    $(document).on("nbxgetcompleted", Admin_plugins_nbxgetCompleted); // assign a completed event for the ajax calls

    // start load all ajax data, continued by js in plugins.js file
    $('.processing').show();

    $('#razortemplate').val('Admin_PluginsList.cshtml');
    nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');

    // function to do actions after an ajax call has been made.
    function Admin_plugins_nbxgetCompleted(e) {

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

        $('#pluginsAdmin_cmdAddNew').unbind("click");
        $('#pluginsAdmin_cmdAddNew').click(function () {
            $('.processing').show();
            $('#razortemplate').val('Admin_pluginsDetail.cshtml');
            nbxget('plugins_adminaddnew', '#nbs_pluginsadminsearch', '#datadisplay');
        });

        if (e.cmd == 'plugins_selectchangedisable' || e.cmd == 'plugins_selectchangehidden') {
            $('.processing').hide();
        };

        if (e.cmd == 'plugins_admin_getlist') {

            $('.processing').hide();

            $('.pluginssearchpanel').show();

            $("#pluginsAdmin_cmdSaveExit").hide();
            $("#pluginsAdmin_cmdSave").hide();
            $("#pluginsAdmin_cmdSaveAs").hide();
            $("#pluginsAdmin_cmdDelete").hide();
            $("#pluginsAdmin_cmdReturn").hide();
            $("#pluginsAdmin_cmdAddNew").show();

            // Move pluginss
            $(".selectmove").hide();
            $(".selectcancel").hide();
            $(".selectrecord").hide();
            $(".savebutton").hide();

            if ($('#searchcategory').val() != '') {
                $(".selectrecord").show();
            }

        $("#ddllistsearchcategory").unbind("change");
            $("#ddllistsearchcategory").change(function() {
                $('#searchcategory').val($("#ddllistsearchcategory").val());
                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });

            $("#chkcascaderesults").unbind("change");
            $("#chkcascaderesults").change(function() {
                if ($("#chkcascaderesults").is(':checked')) {
                    $('#cascade').val("True");
                } else {
                    $('#cascade').val("False");
                }
                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });


            $('.selectrecord').unbind("click");
            $(".selectrecord").click(function() {
                $(".selectrecord").hide();
                $(".selectmove").show();
                $(".selectmove[itemid='" + $(this).attr("itemid") + "']").hide();
                $(".selectcancel[itemid='" + $(this).attr("itemid") + "']").show();
                $("#movepluginsid").val($(this).attr("itemid"));

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
                $("#movetopluginsid").val($(this).attr("itemid"));
                nbxget('plugins_movepluginsadmin', '#nbs_pluginsadminsearch', '#datadisplay');
            });

            $('#pluginsAdmin_searchtext').val($('#searchtext').val());

            // editbutton created by list, so needs to be assigned on each render of list.
            $('.pluginsAdmin_cmdEdit').unbind("click");
            $('.pluginsAdmin_cmdEdit').click(function() {
                $('.processing').show();
                $('#razortemplate').val('Admin_pluginsDetail.cshtml');
                $('#selecteditemid').val($(this).attr('itemid'));
                nbxget('plugins_admin_getdetail', '#nbs_pluginsadminsearch', '#datadisplay');
            });


            $('.selectchangedisable').unbind("click");
            $('.selectchangedisable').click(function () {
                $('.processing').show();
                $('#selecteditemid').val($(this).attr('itemid'));
                if ($(this).hasClass("fa-check-circle")) {
                    $(this).addClass('fa-circle').removeClass('fa-check-circle');
                } else {
                    $(this).addClass('fa-check-circle').removeClass('fa-circle');
                }
                nbxget('plugins_selectchangedisable', '#nbs_pluginsadminsearch');
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
                nbxget('plugins_selectchangehidden', '#nbs_pluginsadminsearch');
            });


            $('.cmdPg').unbind("click");
            $('.cmdPg').click(function() {
                $('.processing').show();
                $('#pagenumber').val($(this).attr('pagenumber'));
                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });

            $('#pluginsAdmin_cmdSearch').unbind("click");
            $('#pluginsAdmin_cmdSearch').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val($('#pluginsAdmin_searchtext').val());

                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });

            $('#pluginsAdmin_cmdReset').unbind("click");
            $('#pluginsAdmin_cmdReset').click(function() {
                $('.processing').show();
                $('#pagenumber').val('1');
                $('#searchtext').val('');
                $("#searchcategory").val('');

                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });

        }

        if (e.cmd == 'plugins_admin_delete') {
            $('.processing').show();
            $('#razortemplate').val('Admin_pluginsList.cshtml');
            $('#selecteditemid').val('');
            nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
        }

        if (e.cmd == 'plugins_admin_save') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            nbxget('plugins_admin_getdetail', '#nbs_pluginsadminsearch', '#datadisplay');
        };

        if (e.cmd == 'plugins_admin_saveexit' || e.cmd == 'plugins_admin_saveas') {
            $("#editlang").val($("#nextlang").val());
            $("#editlanguage").val($("#nextlang").val());
            $('#razortemplate').val('Admin_pluginsList.cshtml');
            $('#selecteditemid').val('');
            nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
        };

        if (e.cmd == 'plugins_movepluginsadmin') {
            $('#razortemplate').val('Admin_pluginsList.cshtml');
            $('#selecteditemid').val('');
            nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
        };
        
        if (e.cmd == 'plugins_getpluginsselectlist') {
            $('.processing').hide();
        };

        if (e.cmd == 'plugins_admin_getdetail'
            || e.cmd == 'plugins_addpluginsmodels'
            || e.cmd == 'plugins_addpluginsoptions'
            || e.cmd == 'plugins_addpluginsoptionvalues'
            || e.cmd == 'plugins_updatepluginsdocs'
            || e.cmd == 'plugins_addpluginscategory'
            || e.cmd == 'plugins_setdefaultcategory'
            || e.cmd == 'plugins_removepluginscategory'
            || e.cmd == 'plugins_populatecategorylist'
            || e.cmd == 'plugins_removeproperty'
            || e.cmd == 'plugins_addproperty'
            || e.cmd == 'plugins_addrelated'
            || e.cmd == 'plugins_removerelated'
            || e.cmd == 'plugins_adminaddnew'
            || e.cmd == 'plugins_updatepluginsimages') {

            // Copy the pluginsid into the selecteditemid (for Add New plugins)
            $('#selecteditemid').val($('#itemid').val());

            $('.actionbuttonwrapper').show();

            $('.processing').hide();

            $('.pluginssearchpanel').hide();
            
            $("#pluginsAdmin_cmdSaveExit").show();
            $("#pluginsAdmin_cmdSave").show();
            $("#pluginsAdmin_cmdSaveAs").show();
            $("#pluginsAdmin_cmdDelete").show();
            $("#pluginsAdmin_cmdReturn").show();
            $("#pluginsAdmin_cmdAddNew").hide();

            $('#datadisplay').children().find('.sortelementUp').click(function () { moveUp($(this).parent()); });
            $('#datadisplay').children().find('.sortelementDown').click(function () { moveDown($(this).parent()); });


            // ---------------------------------------------------------------------------
            // ACTION BUTTONS
            // ---------------------------------------------------------------------------
            $('#pluginsAdmin_cmdReturn').unbind("click");
            $('#pluginsAdmin_cmdReturn').click(function () {
                $('.processing').show();
                $('#razortemplate').val('Admin_pluginsList.cshtml');
                $('#selecteditemid').val('');
                nbxget('plugins_admin_getlist', '#nbs_pluginsadminsearch', '#datadisplay');
            });
            
            $('#pluginsAdmin_cmdSave').unbind("click");
            $('#pluginsAdmin_cmdSave').click(function () {
                $('.actionbuttonwrapper').hide();
                $('.editlanguage').hide();
                $('.processing').show();
                //move data to update postback field
                $('#xmlupdatemodeldata').val($.fn.genxmlajaxitems('#pluginsmodels', '.modelitem'));
                nbxget('plugins_admin_save', '#pluginsdatasection', '#actionreturn');
            });

            $('#pluginsAdmin_cmdDelete').unbind("click");
            $('#pluginsAdmin_cmdDelete').click(function () {
                if (confirm($('#confirmdeletemsg').text())) {
                    $('.actionbuttonwrapper').hide();
                    $('.editlanguage').hide();
                    $('.processing').show();
                    nbxget('plugins_admin_delete', '#nbs_pluginsadminsearch', '#actionreturn');
                }
            });


            // ---------------------------------------------------------------------------
            // MODELS
            // ---------------------------------------------------------------------------
            $('.removemodel').unbind("click");
            $('.removemodel').click(function () { removeelement($(this).parent().parent().parent().parent()); });

            $('input[id*="availabledate"]').datepicker();

            //Add models
            $('#addmodels').unbind("click");
            $('#addmodels').click(function () {
                $('.processing').show();
                $('#addqty').val($('#txtaddmodelqty').val());
                nbxget('plugins_addpluginsmodels', '#nbs_pluginsadminsearch', '#datadisplay'); // load models
            });

            $('#undomodel').unbind("click");
            $('#undomodel').click(function() {
                 undoremove('.modelitem', '#pluginsmodels');
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
        $('#pluginsoptionvalues').children().hide();
        if ($('#pluginsoptions').children('.selected').first().find('input[id*="optionid"]').length > 0) {
            $('#pluginsoptionvalues').children('.' + $('#pluginsoptions').children('.selected').first().find('input[id*="optionid"]').val()).show();
            $('#pluginsoptionvalues').show();
        }
        $('#optionvaluecontrol').show();
    }


    // ---------------------------------------------------------------------------

});

