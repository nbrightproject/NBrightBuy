
$(document).ready(function () {

    // set the default edit language to the current langauge
    $('#editlang').val($('#selectparams #lang').val());

    // get list of records via ajax:  NBrightRazorTemplate_nbxget({command}, {div of data passed to server}, {return html to this div} )
    DiscountCodes_nbxget('getdata', '#selectparams', '#editdata');

    $('.actionbuttonwrapper #cmdsave').click(function () {
        DiscountCodes_nbxget('savedata', '#editdata');
    });

    $('.actionbuttonwrapper #cmdreturn').click(function () {
        $('#selecteditemid').val(''); // clear sleecteditemid.        
        DiscountCodes_nbxget('getdata', '#selectparams', '#editdata');
    });

    $('.actionbuttonwrapper #cmddelete').click(function () {
        if (confirm($('#deletemsg').val())) {
            DiscountCodes_nbxget('deleterecord', '#editdata');            
        }
    });

    $('#addnew').click(function () {
        $('.processing').show();
        $('#newitem').val('new');
        $('#selecteditemid').val('');
        DiscountCodes_nbxget('addnew', '#selectparams', '#editdata');
    });

    $('.selecteditlanguage').click(function () {
        $('#editlang').val($(this).attr('lang')); // alter lang after, so we get correct data record
        DiscountCodes_nbxget('selectlang', '#editdata'); // do ajax call to save current edit form
    });


});

$(document).on("DiscountCodes_nbxgetcompleted", DiscountCodes_nbxgetCompleted); // assign a completed event for the ajax calls


function DiscountCodes_nbxget(cmd, selformdiv, target, selformitemdiv, appendreturn) {
    $('.processing').show();

    $.ajaxSetup({ cache: false });

    var cmdupdate = '/DesktopModules/NBright/NBrightBuy/Providers/PromoProvider/PromoXmlConnector.ashx?cmd=' + cmd;
    var values = '';
    if (selformitemdiv == null) {
        values = $.fn.genxmlajax(selformdiv);
    }
    else {
        values = $.fn.genxmlajaxitems(selformdiv, selformitemdiv);
    }
    var request = $.ajax({
        type: "POST",
        url: cmdupdate,
        cache: false,
        data: { inputxml: encodeURI(values) }
    });

    request.done(function (data) {
        if (data != 'noaction') {
            if (appendreturn == null) {
                $(target).children().remove();
                $(target).html(data).trigger('change');
            } else
                $(target).append(data).trigger('change');

            $.event.trigger({
                type: "DiscountCodes_nbxgetcompleted",
                cmd: cmd
            });
        }
        if (cmd == 'getdata') { // only hide on getdata
            $('.processing').hide();
        }
    });

    request.fail(function (jqXHR, textStatus) {
        alert("Request failed: " + textStatus);
    });
}



function DiscountCodes_nbxgetCompleted(e) {

    if (e.cmd == 'addnew') {
        $('#newitem').val(''); // clear item so if new was just created we don;t create another record
        $('#selecteditemid').val($('#itemid').val()); // move the itemid into the selecteditemid, so page knows what itemid is being edited
        Discountcodes_DetailButtons();
        $('.processing').hide(); // hide on add, not hidden by return
    }

    if (e.cmd == 'deleterecord') {
        $('#selecteditemid').val(''); // clear sleecteditemid, it now doesn;t exists.
        DiscountCodes_nbxget('getdata', '#selectparams', '#editdata');// relist after delete
    }

    if (e.cmd == 'savedata') {
        $('#selecteditemid').val(''); // clear sleecteditemid.        
        DiscountCodes_nbxget('getdata', '#selectparams', '#editdata');// relist after save
    }

    if (e.cmd == 'selectlang') {
        DiscountCodes_nbxget('getdata', '#selectparams', '#editdata'); // do ajax call to get edit form
    }

    // check if we are displaying a list or the detail and do processing.
    if (($('#selecteditemid').val() != '') || (e.cmd == 'addnew')) {
        Discountcodes_DetailButtons();
    } else {
        Discountcodes_ListButtons();
        $('.edititem').click(function () {
            $('.processing').show();
            $('#selecteditemid').val($(this).attr("itemid")); // assign the sleected itemid, so the server knows what item is being edited
            DiscountCodes_nbxget('getdata', '#selectparams', '#editdata'); // do ajax call to get edit form
        });
    }

}

function Discountcodes_DetailButtons() {
    $('#cmdsave').show();
    $('#cmddelete').show();
    $('#cmdreturn').show();
    $('#addnew').hide();
    $('input[datatype="date"]').datepicker(); // assign datepicker to any ajax loaded fields
}

function Discountcodes_ListButtons() {
    $('#cmdsave').hide();
    $('#cmddelete').hide();
    $('#cmdreturn').hide();
    $('#addnew').show();
}

