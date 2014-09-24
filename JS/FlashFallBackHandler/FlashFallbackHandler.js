/**
This script determins if a flash fall back script is necessary, find the requried data, and embeds the script necessary
Written by Jake Jeske 
8/25/2014 v.1.0
*/
var CheckAndExecuteFlashFallBack = function (forceFlash) {
    var force = forceFlash || false;
    if (!document.createElement('video').canPlayType || force) {
        //if cannot play HTML5 video
        //init functions
        var createIeObject = function (url, id) {
            var idStr = "";
            if (id != '')
                idStr = "id='" + id + "' ";
            var div = document.createElement("div");
            div.innerHTML = "<object " + idStr + "classid='clsid:D27CDB6E-AE6D-11cf-96B8-444553540000'><param name='movie' value='" + url + "'/></object>";
            return div.firstChild;
        }
        var getInternetExplorerVersion = function () {
            // Returns the version of Internet Explorer or a -1
            // (indicating the use of another browser).
            var rv = -1; // Return value assumes failure.
            if (navigator.appName == 'Microsoft Internet Explorer') {
                var ua = navigator.userAgent;
                var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
                if (re.exec(ua) != null)
                    rv = parseFloat(RegExp.$1);
            }
            else if (navigator.appName == 'Netscape') {
                var ua = navigator.userAgent;
                var re = new RegExp("Trident/.*rv:([0-9]{1,}[\.0-9]{0,})");
                if (re.exec(ua) != null)
                    rv = parseFloat(RegExp.$1);
            }
            return rv;
        }
        var videos = document.getElementsByTagName('video');
        var IEversion = -1;
        var ActiveXneeded = false;
        if (videos.length > 0) {
            IEversion = getInternetExplorerVersion();
            if (typeof (window.ActiveXObject) == "undefined" && IEversion > 0) {
                ActiveXneeded = true;
            }
        }
        for (var v = 0; v < videos.length; v++) {
            var vid = videos[v];
            var w = vid.hasAttribute('width') ? vid.getAttribute('width') : '';
            var h = vid.hasAttribute('height') ? vid.getAttribute('height') : '';
            var id = vid.hasAttribute('id') ? vid.getAttribute('id') : '';
            var flashSrc = vid.hasAttribute('swfsrc') ? vid.getAttribute('swfsrc') : '';
            if (w != '' && h != '' && flashSrc != '') {
                var classStr = vid.hasAttribute('class') ? vid.getAttribute('class') : '';
                var styleStr = vid.hasAttribute('style') ? vid.getAttribute('class') : '';
                //if has minimum required data to fall back
                if (force && vid.canPlayType) {
                    //stop html5 from autoplaying
                    vid.pause();
                }
                //create flash object
                var flashObject = (IEversion > 0 && IEversion < 11) ? createIeObject(flashSrc, id) : document.createElement("object");
                if (classStr != '')
                    flashObject.class = classStr;
                if (styleStr != '')
                    flashObject.style = styleStr;
                flashObject.height = h;
                flashObject.width = w;
                if (IEversion == -1 || IEversion > 10) {
                    flashObject.setAttribute('type', 'application/x-shockwave-flash');
                    flashObject.setAttribute('data', flashSrc);
                    //if (id != '')
                        //flashObject.setAttribute('id', id);
                }
                //then check for flash options
                var children = vid.childNodes;
                for (var c = 0; c < children.length; c++) {
                    var child = children[c];
                    var childName = child.name;
                    if (childName == 'flashvars') {
                        var varsStr = child.value;
                        if (varsStr != '') {
                            var flashVarParam = document.createElement('param');
                            var append = false;
                            flashVarParam.setAttribute('name', 'FlashVars');
                            if (varsStr.indexOf('=') > 0 && varsStr.indexOf(':') == -1) {
                                flashVarParam.setAttribute('value', varsStr);
                                append = true;
                            }
                            else if (varsStr.indexOf(':') > 0) {
                                var flashVarPairs = varsStr.split(',');
                                var varStr = "";
                                for (var fv = 0; fv < flashVarPairs.length; fv++) {
                                    var fvPair = flashVarPairs[fv];
                                    fvPair = fvPair.replace(':', '=');
                                    if (varsStr == "")
                                        varStr = fvPair;
                                    else
                                        varStr += "&" + fvPair;
                                }
                                if (varStr != "") {
                                    flashVarParam.setAttribute('value', varStr);
                                    append = true;
                                }
                            }
                            if (append)
                                flashObject.appendChild(flashVarParam);
                        }
                    }
                    if (childName == 'params') {
                        var paramsStr = child.value;
                        var paramsPairs = paramsStr.split(',');
                        for (var p = 0; p < paramsPairs.length; p++) {
                            var pair = paramsPairs[p];
                            if (pair != "") {
                                var NameValue = pair.split(':');
                                if (NameValue.length > 1) {
                                    if (NameValue[0] != "" && NameValue[1] != "") {
                                        var param = document.createElement('param');
                                        param.setAttribute('name', NameValue[0]);
                                        param.setAttribute('value', NameValue[1]);
                                        flashObject.appendChild(param);
                                    }
                                }
                            }
                        }
                    }
                }
                if (ActiveXneeded) {
                    flashObject.appendChild(document.createTextNode("!ActiveX is needed to play this video!"));
                }
                if (force)
                    videos[v].parentNode.replaceChild(flashObject, videos[v]);
                else
                    videos[v].appendChild(flashObject);
            }
        }
    }
}

