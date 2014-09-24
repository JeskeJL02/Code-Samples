/*
        This script retreives the secure files for a HTML5 video element, creates it in the DOM, 
    and plays it through a canvas element to keep the user from being able to save the video
    upon right click 'Save As'.
   
        NOTE: Video files are handled by C# and are session specific with changing keys to keep
    others from tring to grab the video directly via the web browser and file route.

     - Jacob Jeske 
*/
var v;
document.addEventListener('DOMContentLoaded', function () {
    var vidID = document.getElementById('VideoID').value;
    //setting up the video element in the DOM
    v = document.createElement('video');
    v.setAttribute('loop', '');
    v.setAttribute('controls', '');
    //creating source elements
    var srcWebm = document.createElement('source');
    srcWebm.setAttribute('src', '/VS/' + vidID + '.webm');
    srcWebm.setAttribute('type', 'video/webm');
    var srcOgv = document.createElement('source');
    srcOgv.setAttribute('src', '/VS/' + vidID + '.ogv');
    srcOgv.setAttribute('type', 'video/ogv');
    var srcMp4 = document.createElement('source');
    srcMp4.setAttribute('src', '/VS/' + vidID + '.mp4');
    srcMp4.setAttribute('type', 'video/mp4');
    v.appendChild(srcWebm);
    v.appendChild(srcMp4);
    v.appendChild(srcOgv);
    //getting the on page canvas element
    var canvas = document.getElementById('VideoCanvas');
    //extracting context
    var context = canvas.getContext('2d');
    //establishing consistency between dimension values
    var cw = canvas.clientWidth;
    var ch = canvas.clientHeight;
    canvas.width = cw;
    canvas.height = ch;
    $("#VideoContainer").css('width', cw + 'px');
    //on play draw each frame into the canvas
    v.addEventListener('play', function () {
        draw(this, context, cw, ch);
    }, false);
}, false);
//draw each frame until video has completed.
function draw(v, c, w, h) {
    if (v.paused || v.ended) return false;
    c.drawImage(v, 0, 0, w, h);
    setTimeout(draw, 20, v, c, w, h);
}
