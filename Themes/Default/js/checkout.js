
        jQuery.fn.extend({
            renameAttr: function (name, newName, removeData) {
                var val;
                return this.each(function () {
                    val = jQuery.attr(this, name);
                    jQuery.attr(this, newName, val);
                    jQuery.removeAttr(this, name);
                    // remove original data
                    if (removeData !== false) {
                        jQuery.removeData(this, name.replace('data-', ''));
                    }
                });
            }
        });

        $(document).ready(function() {

            $('#cmdDeleteCart').unbind();
            $('#cmdDeleteCart').click(function () {
                var msg = $('#cmdClearCart').val();
                if (confirm(msg)) {
                    $('.processing').show();
                    nbxget('cart_clearcart', '#productajaxview');
                }
            });

            $('#cmdRecalcCart').unbind();
            $('#cmdRecalcCart').click(function () {
                $('.processing').show();
                $('#cmdNext').show();
                nbxget('cart_recalculatecart', '.cartdatarow', '', '.quantitycolumn');
            });

            $('#cmdNext').unbind();
            $('#cmdNext').click(function () {
                var cartstep = $('#cartstep').val();
                cartstep = parseInt(cartstep) + 1;
                $('#cartstep').val(cartstep);
                processCartStep('next');
            });
            $('#cmdPrev').unbind();
            $('#cmdPrev').click(function () {
                var cartstep = $('#cartstep').val();
                cartstep = parseInt(cartstep) - 1;
                $('#cartstep').val(cartstep);
                processCartStep('prev');
            });

            $(document).on("nbxgetcompleted", CheckOut_nbxgetCompleted); // assign a completed event for the ajax calls

            // function to do actions after an ajax call has been made.
            function CheckOut_nbxgetCompleted(e) {

                if (e.cmd == 'cart_recalculatecart') {
                    nbxget('cart_rendercartlist', '#productajaxview', '#checkoutdisplaylist');
                }

                if (e.cmd == 'cart_rendercartaddress') {

                    $('#cartactions').show();

                    $('#selectaddress').unbind();
                    $('#selectaddress').change(function() {
                        populateAddressForm($(this).attr('formselector'), $(this).find('option:selected').attr('datavalues'), $(this).find('option:selected').attr('datafields'));
                        $('#billregion').val($('#billaddress_region').val());
                        $('#billaddress_country').trigger("change");
                    });

                    $('#billaddress_country').unbind();
                    $('#billaddress_country').change(function() {
                        $('.checkoutbillformregiondiv').hide();
                        $('.processing').show();
                        nbxget('renderpostdata', '.checkoutbillformcountrydiv', '.checkoutbillformregiondiv');
                    });

                    $('.checkoutbillformregiondiv').unbind();
                    $('.checkoutbillformregiondiv').change(function() {
                        if ($('#billregion').val() != '') {
                            $('#billaddress_region').val($('#billregion').val());
                            $('#billregion').val('')
                        }
                        $('.checkoutbillformregiondiv').show();
                    });


                    $('#selectshipaddress').unbind();
                    $('#selectshipaddress').change(function() {
                        populateAddressForm($(this).attr('formselector'), $(this).find('option:selected').attr('datavalues'), $(this).find('option:selected').attr('datafields'));
                        $('#shipregion').val($('#shipaddress_region').val());
                        $('#shipaddress_country').trigger("change");
                    });

                    $('#shipaddress_country').unbind();
                    $('#shipaddress_country').change(function() {
                        $('.checkoutshipformregiondiv').hide();
                        $('.processing').show();
                        nbxget('renderpostdata', '.checkoutshipformcountrydiv', '.checkoutshipformregiondiv');
                    });

                    $('.checkoutshipformregiondiv').unbind();
                    $('.checkoutshipformregiondiv').change(function() {
                        if ($('#shipregion').val() != '') {
                            $('#shipaddress_region').val($('#shipregion').val());
                            $('#shipregion').val('')
                        }
                        $('.checkoutshipformregiondiv').show();
                    });

                    $('.rblshippingoptions').unbind();
                    $('.rblshippingoptions').change(function() {
                        var selected = $('input[name=extrainfo_rblshippingoptionsradio]:checked');
                        if (selected.val() == '2') {
                            $('.checkoutshipform').show();
                        } else {
                            $('.checkoutshipform').hide();
                        }
                        // disable validation on hidden controls
                        $('input:visible').renameAttr('ignorerequired', 'required');;
                        $('input:hidden').renameAttr('required', 'ignorerequired');
                    });

                    if ($('input[name=extrainfo_rblshippingoptionsradio]:checked').val() == '2') {
                        $('.checkoutshipform').show();
                    } else {
                        $('.checkoutshipform').hide();
                    }

                }

                if (e.cmd == 'cart_rendershipmethod') {

                        $('#cartactions').show();

                        $('.shippingmethodselect').unbind();
                        $('.shippingmethodselect').change(function () {
                            $('.processing').show();
                            nbxget('cart_shippingprovidertemplate', '#checkoutsummary', '#shipprovidertemplates');
                        });

                        //reload shipping provider template on trigger from provider
                        $('.reloadshipprovider').unbind();
                        $('.reloadshipprovider').click(function () {
                            $('.processing').show();
                            nbxget('cart_shippingprovidertemplate', '#checkoutsummary', '#shipprovidertemplates');
                        });
                        //recalc on trigger from provider
                        $('.recalcshipprovider').unbind();
                        $('.recalcshipprovider').click(function () {
                            $('.processing').show();
                            $('#carttemplate').val('CheckoutSummary.cshtml');
                            nbxget('cart_recalculatesummary', '#checkoutsummary');
                        });

                        $('#shipprovidertemplates').unbind();
                        $('#shipprovidertemplates').change(function () {
                            // init js function if included in shipping provider template
                            if (typeof initShippingProviderTemplate == "function") {
                                initShippingProviderTemplate();
                            }
                        });

                        $('.processing').show();
                        nbxget('cart_shippingprovidertemplate', '#checkoutsummary', '#shipprovidertemplates');

                }

                if (e.cmd == 'cart_shippingprovidertemplate') {
                    $('#carttemplate').val('CheckoutSummary.cshtml');
                    $('.processing').show();
                    nbxget('cart_recalculatesummary', '#checkoutsummary');
                }

                if (e.cmd == 'cart_recalculatesummary') {
                    $('.processing').show();
                    nbxget('cart_rendersummary', '#productajaxview', '#checkoutdisplaysummary');
                }

                if (e.cmd == 'cart_clearcart' || e.cmd == 'cart_removefromcart') {
                    $('.processing').show();
                    nbxget('cart_rendercartlist', '#productajaxview', '#checkoutdisplaylist');
                }

                if (e.cmd == 'cart_rendercartlist') {

                    $('.removeitem').unbind();
                    $('.removeitem').click(function() {
                        $('.processing').show();
                        $('#itemcode').val($(this).attr('itemcode'));
                        nbxget('cart_removefromcart', '#productajaxview');
                    });

                    $('#cartactions').show();

                    // if we have a cartempty element hide the action buttons
                    if ($('#cartempty').text() != '') {
                        $('#cartactions').hide();
                    } else {
                        $('#cartactions').show();
                    }

                    $(".quantity").keydown(function(e) {
                        if (e.keyCode == 8 || e.keyCode <= 46) return; // Allow: backspace, delete.
                        if ((e.keyCode >= 35 && e.keyCode <= 39)) return; // Allow: home, end, left, right
                        // Ensure that it is a number and stop the keypress
                        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) e.preventDefault();
                    });

                    $('.qtyminus').unbind();
                    $('.qtyminus').click(function() {
                        var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
                        var newQty = parseInt(oldqty, 10);
                        if (isNaN(newQty)) {
                            newQty = 2;
                        }
                        if (newQty >= 1) {
                            --newQty;
                            $('.itemcode' + $(this).attr('itemcode')).val(newQty);
                        }
                    });
                    $('.qtyplus').unbind();
                    $('.qtyplus').click(function() {
                        var oldqty = $('.itemcode' + $(this).attr('itemcode')).val();
                        var newQty = parseInt(oldqty, 10);
                        if (isNaN(newQty)) {
                            newQty = 0;
                        }
                        ++newQty;
                        $('.itemcode' + $(this).attr('itemcode')).val(newQty);
                    });
                }

                if (e.cmd == 'cart_updatebilladdress') {
                    $('.processing').show();
                    nbxget('cart_updateshipaddress', '.checkoutshipform');
                }

                if (e.cmd == 'cart_updateshipaddress') {
                    $('.processing').show();
                    nbxget('cart_updateshipoption', '#shippingoptions');
                }

                if (e.cmd == 'cart_updateshipoption') {
                    $('#carttemplate').val('CheckoutSummary.cshtml');
                    $('.processing').show();
                    nbxget('cart_rendersummary_ship', '#productajaxview', '#checkoutdisplaysummary');
                }

                if (e.cmd == 'cart_rendersummary' || e.cmd == 'cart_rendersummary_ship' || e.cmd == 'cart_rendersummary_start') {
                    $('#cartactions').show();

                    $('#cmdRecalcSummary').unbind();
                    $('#cmdRecalcSummary').click(function() {
                        $('.processing').show();
                        $('#carttemplate').val('CheckoutSummary.cshtml');
                        nbxget('cart_recalculatesummary', '#checkoutsummary');
                    });

                    $('#cmdOrder').unbind();
                    $('#cmdOrder').click(function () {
                        $('.processing').show();
                        nbxget('cart_redirecttopayment', '#checkoutsummary', '#checkoutpayredirectreturn');
                    });

                    if (e.cmd == 'cart_rendersummary_start' || e.cmd == 'cart_rendersummary_ship') {
                        $('#carttemplate').val('CheckoutShipMethod.cshtml');
                        $('.processing').show();
                        nbxget('cart_rendershipmethod', '#productajaxview', '#checkoutdisplayshipmethod');
                    }
                }

                if (e.cmd == 'cart_redirecttopayment') {
                    $('.processing').show();
                    $('#cmdOrder').hide();
                    window.location.href = $('#checkoutpayredirectreturn').text();
                }

            }

});

function populateAddressForm(selectordiv, datavalues, datafields) {
    // Take the address dropdown data and popluate the address for with it.
    // selectordiv = the selector for the form section that needs popluating
    // datafields = the list of field ids that need popluating (in seq order matching the "data" param)
    // datavalues = the list of data values to be populated.
    if (datavalues != null && datavalues != '') {
        var datarray = datavalues.split(',');
        var fieldarray = datafields.split(',');
        var arrayLength = fieldarray.length;
        for (var i = 0; i < arrayLength; i++) {
            $(selectordiv).find("[id*='" + fieldarray[i] + "']").val(datarray[i]);
        }
    }
}

function processCartStep(buttontype) {

    // show cart list
    if ($('#cartstep').val() == '1') {
        $('.processing').show();
        $('#carttemplate').val('CheckoutList.cshtml');
        $('#cmdDeleteCart').show();
        $('#cmdRecalcCart').show();
        $('#cmdPrev').hide();
        $('#cmdNext').show();
        $('#checkoutdisplaylist').show();
        $('#checkoutdisplayaddr').hide();
        $('#checkoutsummary').hide();
        nbxget('cart_rendercartlist', '#productajaxview', '#checkoutdisplaylist');
    }

    if ($('#cartstep').val() == '2') {
        $('.processing').show();
        $('#carttemplate').val('CheckoutAddress.cshtml');
        $('#cmdDeleteCart').hide();
        $('#cmdRecalcCart').hide();
        $('#cmdPrev').show();
        $('#cmdNext').show();
        $('#checkoutdisplaylist').hide();
        $('#checkoutdisplayaddr').show();
        $('#checkoutsummary').hide();
        nbxget('cart_rendercartaddress', '#productajaxview', '#checkoutdisplayaddr');
    }

    if ($('#cartstep').val() == '3') {
        if (buttontype == 'next') {
            var validator = $("#Form").validate();
            if (validator.form()) {
                $('.processing').show();
                $('#carttemplate').val('CheckoutSummary.cshtml');
                $('#cmdDeleteCart').hide();
                $('#cmdRecalcCart').hide();
                $('#cmdPrev').show();
                $('#cmdNext').hide();
                $('#checkoutdisplaylist').hide();
                $('#checkoutdisplayaddr').hide();
                $('#checkoutsummary').show();
                nbxget('cart_updatebilladdress', '.checkoutbillform');
            } else {
                $('#cartstep').val("2");
            }
        } else {
            $('.processing').show();
            $('#carttemplate').val('CheckoutSummary.cshtml');
            $('#cmdDeleteCart').hide();
            $('#cmdRecalcCart').hide();
            $('#cmdPrev').show();
            $('#cmdNext').hide();
            $('#checkoutdisplaylist').hide();
            $('#checkoutdisplayaddr').hide();
            $('#checkoutsummary').show();
            nbxget('cart_rendersummary_start', '#productajaxview', '#checkoutdisplaysummary');
        }
    }


}


