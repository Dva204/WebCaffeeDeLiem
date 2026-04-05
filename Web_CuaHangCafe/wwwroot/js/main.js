/*  ---------------------------------------------------
Template Name: Ashion
Description: Ashion ecommerce template
Author: Colorib
Author URI: https://colorlib.com/
Version: 1.0
Created: Colorib
---------------------------------------------------------  */

'use strict';

(function ($) {

    /*------------------
        Preloader
    --------------------*/
    $(window).on('load', function () {
        $(".loader").fadeOut();
        $("#preloder").delay(200).fadeOut("slow");

        /*------------------
            Product filter
        --------------------*/
        $('.filter__controls li').on('click', function () {
            $('.filter__controls li').removeClass('active');
            $(this).addClass('active');
        });
        if ($('.property__gallery').length > 0) {
            var containerEl = document.querySelector('.property__gallery');
            var mixer = mixitup(containerEl);
        }
    });

    /*------------------
        Background Set
    --------------------*/
    $('.set-bg').each(function () {
        var bg = $(this).data('setbg');
        $(this).css('background-image', 'url(' + bg + ')');
    });

    //Search Switch
    $('.search-switch').on('click', function () {
        $('.search-model').fadeIn(400);
    });

    $('.search-close-switch').on('click', function () {
        $('.search-model').fadeOut(400, function () {
            $('#search-input').val('');
        });
    });

    //Canvas Menu
    $(".canvas__open").on('click', function () {
        $(".offcanvas-menu-wrapper").addClass("active");
        $(".offcanvas-menu-overlay").addClass("active");
    });

    $(".offcanvas-menu-overlay, .offcanvas__close").on('click', function () {
        $(".offcanvas-menu-wrapper").removeClass("active");
        $(".offcanvas-menu-overlay").removeClass("active");
    });

    /*------------------
        Navigation
    --------------------*/
    $(".header__menu").slicknav({
        prependTo: '#mobile-menu-wrap',
        allowParentLinks: true
    });

    /*------------------
        Accordin Active
    --------------------*/
    $('.collapse').on('shown.bs.collapse', function () {
        $(this).prev().addClass('active');
    });

    $('.collapse').on('hidden.bs.collapse', function () {
        $(this).prev().removeClass('active');
    });

    /*--------------------------
        Banner Slider
    ----------------------------*/
    $(".banner__slider").owlCarousel({
        loop: true,
        margin: 0,
        items: 1,
        dots: true,
        smartSpeed: 1200,
        autoHeight: false,
        autoplay: true
    });

    /*--------------------------
        Product Details Slider
    ----------------------------*/
    $(".product__details__pic__slider").owlCarousel({
        loop: false,
        margin: 0,
        items: 1,
        dots: false,
        nav: true,
        navText: ["<i class='arrow_carrot-left'></i>", "<i class='arrow_carrot-right'></i>"],
        smartSpeed: 1200,
        autoHeight: false,
        autoplay: false,
        mouseDrag: false,
        startPosition: 'URLHash'
    }).on('changed.owl.carousel', function (event) {
        var indexNum = event.item.index + 1;
        product_thumbs(indexNum);
    });

    function product_thumbs(num) {
        var thumbs = document.querySelectorAll('.product__thumb a');
        thumbs.forEach(function (e) {
            e.classList.remove("active");
            if (e.hash.split("-")[1] == num) {
                e.classList.add("active");
            }
        })
    }


    /*------------------
        Magnific
    --------------------*/
    $('.image-popup').magnificPopup({
        type: 'image'
    });


    $(".nice-scroll").niceScroll({
        cursorborder: "",
        cursorcolor: "#dddddd",
        boxzoom: false,
        cursorwidth: 5,
        background: 'rgba(0, 0, 0, 0.2)',
        cursorborderradius: 50,
        horizrailenabled: false
    });

    /*------------------
        CountDown
    --------------------*/
    // For demo preview start
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    var yyyy = today.getFullYear();

    if (mm == 12) {
        mm = '01';
        yyyy = yyyy + 1;
    } else {
        mm = parseInt(mm) + 1;
        mm = String(mm).padStart(2, '0');
    }
    var timerdate = mm + '/' + dd + '/' + yyyy;
    // For demo preview end


    // Uncomment below and use your date //

    /* var timerdate = "2020/12/30" */

    $("#countdown-time").countdown(timerdate, function (event) {
        $(this).html(event.strftime("<div class='countdown__item'><span>%D</span> <p>Day</p> </div>" + "<div class='countdown__item'><span>%H</span> <p>Hour</p> </div>" + "<div class='countdown__item'><span>%M</span> <p>Min</p> </div>" + "<div class='countdown__item'><span>%S</span> <p>Sec</p> </div>"));
    });

    /*-------------------
        Range Slider
    --------------------- */
    var rangeSlider = $(".price-range"),
        minamount = $("#minamount"),
        maxamount = $("#maxamount"),
        minPrice = rangeSlider.data('min'),
        maxPrice = rangeSlider.data('max');
    rangeSlider.slider({
        range: true,
        min: minPrice,
        max: maxPrice,
        values: [minPrice, maxPrice],
        slide: function (event, ui) {
            minamount.val('$' + ui.values[0]);
            maxamount.val('$' + ui.values[1]);
        }
    });
    minamount.val('$' + rangeSlider.slider("values", 0));
    maxamount.val('$' + rangeSlider.slider("values", 1));

    /*------------------
        Single Product
    --------------------*/
    $('.product__thumb .pt').on('click', function () {
        var imgurl = $(this).data('imgbigurl');
        var bigImg = $('.product__big__img').attr('src');
        if (imgurl != bigImg) {
            $('.product__big__img').attr({ src: imgurl });
        }
    });

    /*-------------------
        Quantity change
    --------------------- */
    var proQty = $('.pro-qty');
    proQty.prepend('<span class="dec qtybtn">-</span>');
    proQty.append('<span class="inc qtybtn">+</span>');
    proQty.on('click', '.qtybtn', function () {
        var $button = $(this);
        var oldValue = $button.parent().find('input').val();
        if ($button.hasClass('inc')) {
            var newVal = parseFloat(oldValue) + 1;
        } else {
            // Don't allow decrementing below zero
            if (oldValue > 0) {
                var newVal = parseFloat(oldValue) - 1;
            } else {
                newVal = 0;
            }
        }
        $button.parent().find('input').val(newVal);
    });

    /*-------------------
        Radio Btn
    --------------------- */
    $(".size__btn label").on('click', function () {
        $(".size__btn label").removeClass('active');
        $(this).addClass('active');
    });


    /* ============================================
       ADD TO CART — hiệu ứng đầy đủ
       ============================================ */

    // Tạo toast 1 lần, dùng lại
    if ($('.cart-toast').length === 0) {
        $('body').append('<div class="cart-toast"><i class="fas fa-check-circle"></i><span>Đã thêm vào giỏ hàng</span></div>');
    }

    $(document).on("click", ".add-to-cart", function (e) {
        e.preventDefault();

        var btn = $(this);
        var productId = btn.data("id");

        // Feedback ngay lập tức trên nút
        btn.addClass('adding');
        setTimeout(function () { btn.removeClass('adding'); }, 300);

        $.ajax({
            url: "/cart/add",
            type: "POST",
            data: { id: productId, quantity: 1 },
            success: function (res) {
                if (res.requireLogin) {
                    window.location.href = res.redirectUrl;
                    return;
                }
                if (res.success) {
                    // Cập nhật badge
                    $(".cart-count").text(res.count);

                    // Hiệu ứng dot bay vào giỏ
                    flyDotToCart(btn);

                    // Hiệu ứng giỏ hàng nảy
                    bumpCart();

                    // Toast thông báo
                    showCartToast();
                }
            }
        });
    });

    /* -- Dot nhỏ bay từ sản phẩm đến giỏ -- */
    function flyDotToCart(btn) {
        var cartBtn = $("#cartWidgetBtn");
        if (cartBtn.length === 0) cartBtn = $(".header__right__widget");
        if (cartBtn.length === 0) return;

        // Điểm XUẤT PHÁT: tâm của thẻ sản phẩm (.product__item)
        var productItem = btn.closest(".product__item");
        var startEl = productItem.length ? productItem : btn;
        var startOffset = startEl.offset();
        var startX = startOffset.left + startEl.outerWidth() / 2;
        var startY = startOffset.top + startEl.outerHeight() / 2;

        // Điểm KẾT THÚC: tâm của icon giỏ hàng
        var endOffset = cartBtn.offset();
        var endX = endOffset.left + cartBtn.outerWidth() / 2;
        var endY = endOffset.top + cartBtn.outerHeight() / 2;

        // Tạo nhiều dot để tạo cảm giác "vệt"
        var dotCount = 3;
        for (var i = 0; i < dotCount; i++) {
            (function (delay) {
                setTimeout(function () {
                    var dot = $('<div class="fly-dot"></div>').css({
                        left: startX - 7,
                        top: startY - 7
                    }).appendTo('body');

                    // Dùng requestAnimationFrame để animate mượt
                    var startTime = null;
                    var duration = 550;

                    // Điểm điều khiển cung bay (bezier thủ công)
                    var ctrlX = (startX + endX) / 2 + (Math.random() - 0.5) * 100;
                    var ctrlY = Math.min(startY, endY) - 80 - Math.random() * 60;

                    function animateDot(timestamp) {
                        if (!startTime) startTime = timestamp;
                        var progress = Math.min((timestamp - startTime) / duration, 1);
                        var ease = 1 - Math.pow(1 - progress, 3); // ease-out cubic

                        // Quadratic bezier
                        var t = ease;
                        var x = Math.pow(1 - t, 2) * startX + 2 * (1 - t) * t * ctrlX + Math.pow(t, 2) * endX;
                        var y = Math.pow(1 - t, 2) * startY + 2 * (1 - t) * t * ctrlY + Math.pow(t, 2) * endY;

                        var scale = 1 - progress * 0.6;
                        dot.css({
                            left: x - 7,
                            top: y - 7,
                            transform: 'scale(' + scale + ')',
                            opacity: 1 - progress * 0.3
                        });

                        if (progress < 1) {
                            requestAnimationFrame(animateDot);
                        } else {
                            dot.remove();
                        }
                    }
                    requestAnimationFrame(animateDot);
                }, delay);
            })(i * 80);
        }
    }

    /* -- Hiệu ứng bump trên icon giỏ hàng -- */
    function bumpCart() {
        var cartWidget = $("#cartWidgetBtn");
        if (cartWidget.length === 0) return;

        cartWidget.removeClass('cart--bump');
        // reflow để reset animation
        void cartWidget[0].offsetWidth;
        cartWidget.addClass('cart--bump');

        cartWidget.one('animationend webkitAnimationEnd', function () {
            cartWidget.removeClass('cart--bump');
        });
        // Fallback nếu event không trigger
        setTimeout(function () {
            cartWidget.removeClass('cart--bump');
        }, 600);
    }

    /* -- Toast thông báo nhỏ -- */
    var toastTimer;
    function showCartToast() {
        var toast = $('.cart-toast');
        clearTimeout(toastTimer);
        toast.addClass('show');
        toastTimer = setTimeout(function () {
            toast.removeClass('show');
        }, 2200);
    }

})(jQuery);