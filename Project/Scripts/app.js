const menu = document.querySelector('.menu');
menu.addEventListener('click',function(){
    document.body.classList.toggle('menu-open');
});
window.addEventListener('click', function(e){ 
    if(e.target == document.querySelector('.overlay') || e.target == document.querySelector('.fa-times')){
        document.body.classList.toggle('menu-open');
    }
});
const submenu = document.querySelector('li#parent-menu');
submenu.addEventListener('click', (e) => {
    var child = submenu.childElementCount; // 2
    var a = submenu.childNodes[1].lastChild;
    if(a.classList.contains("fa-angle-right"))
    {
        a.classList.remove('fa-angle-right');
        a.classList.add('fa-angle-down');
    }
    else{
        a.classList.add('fa-angle-right');
        a.classList.remove('fa-angle-down');
    }
    submenu.childNodes[3].classList.toggle('active');
});
// click function
var func = document.querySelector('.function');
func.addEventListener('click',function(){
    this.classList.toggle('active');
});
// pop up
function showPopup(title, msg, bool) {
    if (bool == true) {
        $('#popup-header').html(title);
        $('#popup-content').html(msg);
        document.querySelector('.message-container').classList.toggle('close-pop');
    }
    else {
        document.querySelector('.message-container').classList.toggle('close-pop');
    }
}
// close pop
var closepop = document.querySelector('#close-popup');
closepop.addEventListener('click', function () {
    document.querySelector('.message-container').classList.toggle('close-pop');
});

var click_fb = document.querySelector('#clickfb');
var phanhoi = document.querySelector('.phanhoi-container');
var closePhanhoi = document.querySelector('.close-phanhoi');
if (click_fb) {
    click_fb.addEventListener('click', function () {
        phanhoi.classList.toggle('actives-phanhoi');
    });
}
if (phanhoi) {
    phanhoi.addEventListener('click', function (e) {
        var div = e.target;
        if (div.classList.contains('phanhoi-container')) {
            phanhoi.classList.remove('actives-phanhoi');
        }
    });
}
if (closePhanhoi) {
    closePhanhoi.addEventListener('click', function (e) {
        phanhoi.classList.remove('actives-phanhoi');
    });
}

new Glide(".images",{
    type: 'carousel',
    perView: 4,
    focusAt: 'center',
    gap : 40,
    breakpoints: {
        1200:{
            perView : 3
        },
        800:{
            perView : 2
        },
        600:{
            perView : 1
        }
    }
}).mount();