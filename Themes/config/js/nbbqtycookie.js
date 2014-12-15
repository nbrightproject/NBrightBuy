    //---------------------------------------------------------------
    //----- functions required to deal with qty cookie --------------
    //---------------------------------------------------------------
    function parseDic(arrayData) {
        var data = "";
        for (key in arrayData) {
            data += key + ":" + arrayData[key] + "*"
        }
        return data;
    };

    function buildDic(strData) {
        if (strData === undefined) strData = '';
        if (strData === null) strData = '';
        var data = {};
        var list = strData.split('*');
        for (c = 0; c < list.length; c++) {
            var list2 = list[c].split(':');
            if (list2.length == 2) {
                data[list2[0]] = list2[1];
            }
        }
        return data;
    };

    function displayQty() {
        var arrayData = buildDic(readCookie('nbrightbuy_qtyselected'));
        $('.icon-plus').each(function (index) {
            var key =$(this).attr('productid') + '-' +  $(this).attr('itemid');
            if (arrayData[key] !== undefined) {
                $('input[modelid="' + $(this).attr('itemid') + '"]').val(arrayData[key])
            }
        });
    };

    function createCookie(name, value) {
        document.cookie = name + "=" + value + "; path=/";
    };

    function readCookie(name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    };
    
    function eraseCookie(name) {
        createCookie(name, "", -1);
        location.reload();
    };

    function SaveToCookie(obj) {
        var qtyvalues = buildDic(readCookie('nbrightbuy_qtyselected'));
        qtyvalues[obj.attr('productid') + "-" + obj.attr('itemid')] = $('.jqselector' + obj.attr('itemid') + ' > .quantity').val();
        createCookie('nbrightbuy_qtyselected', parseDic(qtyvalues), 1);
        //alert(readCookie('nbrightbuy_qtyselected'));
    };
