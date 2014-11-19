/** ********************************************** **
	@Author			Dorin Grigoras
	@Website		www.stepofweb.com
	@Last Update	Sunday, March 16, 2014

	NOTE! 	Do not change anything here if you want to
			be able to update in the future! Please use
			your custom script (eg. custom.js).


	TABLE CONTENTS
	-------------------------------
	Top Nav
	Maximage Slider
	Animate
	OWL Carousel
	Popover
	Lightbox
	ScrollTo
	Isotope
	Toggle
	Placeholder
	Google Map
	Nicescroll
	Lazyload

	COUNT TO
	FITVIDS
	WAIT FOR IMAGES [used by masonry]
	LAZYLOAD
 **************************************************************** **/
	var is_msie = (navigator.appVersion.indexOf("MSIE")!=-1) ? true : false;

	/* Init */
	jQuery(window).ready(function () {
		Alkaline();
	});


/** Core
 **************************************************************** **/
	function Alkaline() {
		_topNav();
		_maximageSlider();
		_animate();
		_owl_carousel();
		_popover();
		_lightbox();
		_scrollTo();
		_isotope();
		_toggle();
		_placeholder();
		_niceScroll();
		_lazyImage();

		/** Bootstrap Tooltip **/ 
		jQuery("a[data-toggle=tooltip]").tooltip();
		
		/** Fitvids [Youtube|Vimeo] **/
		if(jQuery("#maximage").length < 1) { // disable fitvids if maximage slider is present
			jQuery("body").fitVids(); 
		}


		/** 
			price slider 

			<script type="text/javascript" charset="utf-8">
				var slider_config = { from: 10, to: 500, heterogeneity: ['50/100', '75/250'], step: 10, dimension: '&nbsp;$', skin: 'round_plastic' };
			</script>
		**/
		if(jQuery().slider && jQuery(".price-slider").length > 0) {
			jQuery("#Slider2").slider(slider_config);
		}
	}


/** Top Nav
 **************************************************************** **/
	function _topNav() {var addActiveClass = false;

		// Menu Close Bugfix
		jQuery("button.btn-mobile").bind("click", function(e) {
			if(jQuery("#header .navbar-collapse").hasClass("in")) {
				jQuery("#header .navbar-collapse").removeClass("in");
				return false;
			}
		});

		// Responsive Menu
		jQuery("#topMain li.dropdown > a, #topMain li.dropdown-submenu > a").bind("click", function(e) {
			e.preventDefault();

			if(jQuery(window).width() > 979) {
				return;
			}


			addActiveClass = jQuery(this).parent().hasClass("resp-active");
			jQuery("#topMain").find(".resp-active").removeClass("resp-active");

			if(!addActiveClass) {
				jQuery(this).parents("li").addClass("resp-active");
			}

			return;

		});

		jQuery("button.btn-mobile").bind("click", function() {
			_reset(); // reset visible items
		});

		// shop cart
		jQuery("div.topShopCart").bind("click", function(e) {
			var _tc = jQuery("#topCart");

			_reset(); // reset visible items

			if(_tc.is(":visible")) {
				_tc.fadeOut(150);
				_overlay('remove');
			} else {
				_tc.fadeIn(150);
				_overlay('append');
			}
		});


		// top search
		jQuery("button.topSearchBtn").bind("click", function(e) {
			var _tc = jQuery("#topSearch");

			_reset(); // reset visible items

			if(_tc.is(":visible")) {
				_tc.fadeOut(150);
				_overlay('remove');
			} else {
				_tc.fadeIn(150);
				_overlay('append');
			}
		});

		// 'esc' key
		jQuery(document).keydown(function(e) {
			var code = e.keyCode ? e.keyCode : e.which;
			if(code == 27) {
				_reset(); // reset visible items
			}
		});

	}



/** Maximage Slider
 **************************************************************** **/
	function _maximageSlider() {

		if(jQuery('#slider').length > 0) {

			jQuery('#slider').maximage({
				cycleOptions: {
					fx: 		'fade',
					speed: 		1000, // Has to match the speed for CSS transitions in jQuery.maximage.css (lines 30 - 33)
					timeout: 	0,
					prev: 		'#slider_prev',
					next: 		'#slider_next',
					pause: 		1,

					before: function(last,current){
						if(!is_msie) {
							// Start HTML5 video when you arrive
							if(jQuery(current).find('video').length > 0) jQuery(current).find('video')[0].play();
						}
					},

					after: function(last,current){
						if(!is_msie) {
							// Pauses HTML5 video when you leave it
							if(jQuery(last).find('video').length > 0) jQuery(last).find('video')[0].pause();
						}
					}
				},

				onFirstImageLoaded: function(){
					jQuery('#cycle-loader').hide();
					jQuery('#slider').fadeIn('fast');
				}
			});

			// Helper function to Fill and Center the HTML5 Video
			jQuery('video,object').maximage('maxcover');

			// To show it is dynamic html text
			jQuery('.in-slide-content').delay(1200).fadeIn();

		}

	}


/** Overlay - used by various function like loading/cart/etc
 **************************************************************** **/
	function _overlay(what) {

		if(what == 'append') {

			jQuery("#overlay").remove();
			jQuery("body").append('<div id="overlay"></div>');
			jQuery(this).fadeIn(500);
			jQuery("#overlay").fadeIn();
			_overlayResetBind();

		} else {

			jQuery("#overlay").fadeOut(500, function() {
				jQuery(this).remove();
			});

		}

	}

	// on overlay click, hide all elements like topCart, centerBox, etc.
	// It's much better and easy instead of window.click
	function _overlayResetBind() {
		jQuery("#overlay").bind("click", function(e) {
			_reset();
		});
	}


/** Reset All
 **************************************************************** **/
	function _reset() {
		// Hide Topcart if visible
		var _tc = jQuery("#topCart");
		if(_tc.is(":visible")) {
			_tc.fadeOut(150);
			_overlay('remove');
		}

		// Hide Center Box
		var _cb = jQuery(".centerBox");
		if(_cb.is(":visible")) {
			_cb.fadeOut(150);
			_overlay('remove');
		}

		// Hide Top Search if visible
		var _tc = jQuery("#topSearch");
		if(_tc.is(":visible")) {
			_tc.fadeOut(150);
			_overlay('remove');
		}

		if(jQuery("#header .navbar-collapse").hasClass("in")) {
			jQuery("#header .navbar-collapse").removeClass("in");
		}
	}


/** Animate
 **************************************************************** **/
	function _animate() {

		// Bootstrap Progress Bar
		jQuery("[data-appear-progress-animation]").each(function() {
			var $_t = jQuery(this);

			if(jQuery(window).width() > 767) {

				$_t.appear(function() {
					_delay = 1;

					if($_t.attr("data-appear-progress-animation-delay")) {
						_delay = $_t.attr("data-appear-progress-animation-delay");
					}

					if(_delay > 1) {
						$_t.css("animation-delay", _delay + "ms");
					}

					$_t.addClass($_t.attr("data-appear-progress-animation"));

					setTimeout(function() {

						$_t.addClass("animation-visible");

					}, _delay);

				}, {accX: 0, accY: -150});

			} else {

				$_t.addClass("animation-visible");

			}

		});


		jQuery("[data-appear-progress-animation]").each(function() {
			var $_t = jQuery(this);

			$_t.appear(function() {

				var _delay = ($_t.attr("data-appear-animation-delay") ? $_t.attr("data-appear-animation-delay"): 1);

				if(_delay > 1) {
					$_t.css("animation-delay", _delay + "ms");
				}

				$_t.addClass($_t.attr("data-appear-animation"));
				setTimeout(function() {

					$_t.animate({"width": $_t.attr("data-appear-progress-animation")}, 1000, "easeOutQuad", function() {
						$_t.find(".progress-bar-tooltip").animate({"opacity": 1}, 500, "easeOutQuad");
					});

				}, _delay);

			}, {accX: 0, accY: -50});

		});


		// Count To
		jQuery(".countTo [data-to]").each(function() {
			var $_t = jQuery(this);

			$_t.appear(function() {

				$_t.countTo();

			}, {accX:0, accY:-150});

		});


		/* Knob Circular Bar */
		if(jQuery().knob) {

			jQuery(".knob").knob();

		}


		/* Animation */
		jQuery('.animate_from_top').each(function () {
			jQuery(this).appear(function() {
				jQuery(this).delay(150).animate({opacity:1,top:"0px"},1000);
			});	
		});

		jQuery('.animate_from_bottom').each(function () {
			jQuery(this).appear(function() {
				jQuery(this).delay(150).animate({opacity:1,bottom:"0px"},1000);
			});	
		});


		jQuery('.animate_from_left').each(function () {
			jQuery(this).appear(function() {
				jQuery(this).delay(150).animate({opacity:1,left:"0px"},1000);
			});	
		});


		jQuery('.animate_from_right').each(function () {
			jQuery(this).appear(function() {
				jQuery(this).delay(150).animate({opacity:1,right:"0px"},1000);
			});	
		});

		jQuery('.animate_fade_in').each(function () {
			jQuery(this).appear(function() {
				jQuery(this).delay(350).animate({opacity:1,right:"0px"},1000);
			});	
		});
	}



/** OWL Carousel
 **************************************************************** **/
	function _owl_carousel() {

		var total = jQuery("div.owl-carousel").length,
			count = 0;

		jQuery("div.owl-carousel").each(function() {

			var slider 		= jQuery(this);
			var options 	= slider.attr('data-plugin-options');

			var defaults = {
				items: 					5,
				itemsCustom: 			false,
				itemsDesktop: 			[1199,4],
				itemsDesktopSmall: 		[980,3],
				itemsTablet: 			[768,2],
				itemsTabletSmall: 		false,
				itemsMobile: 			[479,1],
				singleItem: 			true,
				itemsScaleUp: 			false,

				slideSpeed: 			200,
				paginationSpeed: 		800,
				rewindSpeed: 			1000,

				autoPlay: 				false,
				stopOnHover: 			false,

				navigation: 			false,
				navigationText: [
									'<i class="fa fa-chevron-left"></i>',
									'<i class="fa fa-chevron-right"></i>'
								],
				rewindNav: 				true,
				scrollPerPage: 			false,

				pagination: 			true,
				paginationNumbers: 		false,

				responsive: 			true,
				responsiveRefreshRate: 	200,
				responsiveBaseWidth: 	window,

				baseClass: 				"owl-carousel",
				theme: 					"owl-theme",

				lazyLoad: 				false,
				lazyFollow: 			true,
				lazyEffect: 			"fade",

				autoHeight: 			false,

				jsonPath: 				false,
				jsonSuccess: 			false,

				dragBeforeAnimFinish: 	true,
				mouseDrag: 				true,
				touchDrag: 				true,

				transitionStyle: 		false,

				addClassActive: 		false,

				beforeUpdate: 			false,
				afterUpdate: 			false,
				beforeInit: 			false,
				afterInit: 				false,
				beforeMove: 			false,
				afterMove: 				false,
				afterAction: 			false,
				startDragging: 			false,
				afterLazyLoad: 			false
			}

			var config = jQuery.extend({}, defaults, options, slider.data("plugin-options"));
			slider.owlCarousel(config).addClass("owl-carousel-init");
		});
	}



/** Popover
 **************************************************************** **/
	function _popover() {

			jQuery("a[data-toggle=popover]").bind("click", function(e) {
				e.preventDefault();
			});

			var isVisible 	= false,
				clickedAway = false;

			jQuery("a[data-toggle=popover], button[data-toggle=popover]").popover({

					html: true,
					trigger: 'manual'

				}).click(function(e) {

					jQuery(this).popover('show');
					jQuery('.popover-title').append('<button type="button" class="close">&times;</button>');
					clickedAway = false;
					isVisible = true;
					e.preventDefault();

				});

				jQuery(document).click(function(e) {

					if(isVisible & clickedAway) {

						jQuery("a[data-toggle=popover], button[data-toggle=popover]").popover('hide')
						isVisible = clickedAway = false;

					} else {

						clickedAway = true;

					}

				});

			jQuery("a[data-toggle=popover], button[data-toggle=popover]").popover({

				html: true,
				trigger: 'manual'

			}).click(function(e) {

				$(this).popover('show');
				$('.popover-title').append('<button type="button" class="close">&times;</button>');
				$('.close').click(function(e){

					jQuery("a[data-toggle=popover], button[data-toggle=popover]").popover('hide');

				});

				e.preventDefault();
			});

	}



/** LightBox
 **************************************************************** **/
	function _lightbox() {

		if(typeof(jQuery.magnificPopup) == "undefined") {
			return false;
		}

		jQuery.extend(true, jQuery.magnificPopup.defaults, {
			tClose: 		'Close',
			tLoading: 		'Loading...',

			gallery: {
				tPrev: 		'Previous',
				tNext: 		'Next',
				tCounter: 	'%curr% / %total%'
			},

			image: 	{ 
				tError: 	'Image not loaded!' 
			},

			ajax: 	{ 
				tError: 	'Content not loaded!' 
			}
		});

		jQuery(".lightbox").each(function() {

			var _t 			= jQuery(this),
				options 	= _t.attr('data-plugin-options'),
				config		= {},
				defaults 	= {
					type: 				'image',
					fixedContentPos: 	false,
					fixedBgPos: 		false,
					mainClass: 			'mfp-no-margins mfp-with-zoom',
					image: {
						verticalFit: 	true
					},

					zoom: {
						enabled: 		false,
						duration: 		300
					},

					gallery: {
						enabled: false,
						navigateByImgClick: true,
						preload: 			[0,1],
						arrowMarkup: 		'<button title="%title%" type="button" class="mfp-arrow mfp-arrow-%dir%"></button>',
						tPrev: 				'Previou',
						tNext: 				'Next',
						tCounter: 			'<span class="mfp-counter">%curr% / %total%</span>'
					},
				};

			if(_t.data("plugin-options")) {
				config = jQuery.extend({}, defaults, options, _t.data("plugin-options"));
			}

			jQuery(this).magnificPopup(config);

		});
	}



/** ScrollTo
 **************************************************************** **/
	function _scrollTo() {

		jQuery("a.scrollTo").bind("click", function(e) {
			e.preventDefault();

			var href = jQuery(this).attr('href');

			if(href != '#') {
				jQuery('html,body').animate({scrollTop: jQuery(href).offset().top - 60}, 1000, 'easeInOutExpo');
			}
		});

		jQuery("a.toTop").bind("click", function(e) {
			e.preventDefault();
			jQuery('html,body').animate({scrollTop: 0}, 1000, 'easeInOutExpo');
		});
	}

	
	
/** Isotope
 **************************************************************** **/
	function _isotope() {

		jQuery("ul.isotope-filter").each(function() {

			var _el		 		= jQuery(this),
				destination 	= jQuery("ul.sort-destination[data-sort-id=" + jQuery(this).attr("data-sort-id") + "]");

			if(destination.get(0)) {

				jQuery(window).load(function() {

					destination.isotope({
						itemSelector: 	"li",
						layoutMode: 	'sloppyMasonry'
					});

					_el.find("a").click(function(e) {

						e.preventDefault();

						var $_t 	= jQuery(this),
							sortId 	= $_t.parents(".sort-source").attr("data-sort-id"),
							filter 	= $_t.parent().attr("data-option-value");

						_el.find("li.active").removeClass("active");
						$_t.parent().addClass("active");

						destination.isotope({
							filter: filter
						});

						jQuery(".sort-source-title[data-sort-id=" + sortId + "] strong").html($_t.html());
						return false;

					});

				});

			}

		});


		jQuery(window).load(function() {

			jQuery("ul.isotope").addClass('fadeIn');

		});
	}



/** Toggle
 **************************************************************** **/
	function _toggle() {

		var $_t = this,
			previewParClosedHeight = 25;

		jQuery("div.toggle.active > p").addClass("preview-active");
		jQuery("div.toggle.active > div.toggle-content").slideDown(400);
		jQuery("div.toggle > label").click(function(e) {

			var parentSection 	= jQuery(this).parent(),
				parentWrapper 	= jQuery(this).parents("div.toogle"),
				previewPar 		= false,
				isAccordion 	= parentWrapper.hasClass("toogle-accordion");

			if(isAccordion && typeof(e.originalEvent) != "undefined") {
				parentWrapper.find("div.toggle.active > label").trigger("click");
			}

			parentSection.toggleClass("active");

			if(parentSection.find("> p").get(0)) {

				previewPar 					= parentSection.find("> p");
				var previewParCurrentHeight = previewPar.css("height");
				var previewParAnimateHeight = previewPar.css("height");
				previewPar.css("height", "auto");
				previewPar.css("height", previewParCurrentHeight);

			}

			var toggleContent = parentSection.find("> div.toggle-content");

			if(parentSection.hasClass("active")) {

				jQuery(previewPar).animate({height: previewParAnimateHeight}, 350, function() {jQuery(this).addClass("preview-active");});
				toggleContent.slideDown(350);

			} else {

				jQuery(previewPar).animate({height: previewParClosedHeight}, 350, function() {jQuery(this).removeClass("preview-active");});
				toggleContent.slideUp(350);

			}

		});
	}



/** Placeholder
 **************************************************************** **/
	function _placeholder() {

		//check for IE
		if(navigator.appVersion.indexOf("MSIE")!=-1) {

			jQuery('[placeholder]').focus(function() {

				var input = jQuery(this);
				if (input.val() == input.attr('placeholder')) {
					input.val('');
					input.removeClass('placeholder');
				}

			}).blur(function() {

				var input = jQuery(this);
				if (input.val() == '' || input.val() == input.attr('placeholder')) {
				input.addClass('placeholder');
				input.val(input.attr('placeholder'));
				}

			}).blur();

		}

	}



/**	Google Map
 **************************************************************** **/
	function contactMap() {

		var latLang = new google.maps.LatLng($googlemap_latitude,$googlemap_longitude);

		var mapOptions = {
			zoom:$googlemap_zoom,
			center: latLang,
			disableDefaultUI: false,
			navigationControl: false,
			mapTypeControl: false,
			scrollwheel: false,
			// styles: styles,
			mapTypeId: google.maps.MapTypeId.ROADMAP
		};

		var map = new google.maps.Map(document.getElementById('gmap'), mapOptions);
		google.maps.event.trigger(map, 'resize');
		map.setZoom( map.getZoom() );

		var marker = new google.maps.Marker({
			icon: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACcAAAArCAYAAAD7YZFOAAAACXBIWXMAAAsTAAALEwEAmpwYAAAKT2lDQ1BQaG90b3Nob3AgSUNDIHByb2ZpbGUAAHjanVNnVFPpFj333vRCS4iAlEtvUhUIIFJCi4AUkSYqIQkQSoghodkVUcERRUUEG8igiAOOjoCMFVEsDIoK2AfkIaKOg6OIisr74Xuja9a89+bN/rXXPues852zzwfACAyWSDNRNYAMqUIeEeCDx8TG4eQuQIEKJHAAEAizZCFz/SMBAPh+PDwrIsAHvgABeNMLCADATZvAMByH/w/qQplcAYCEAcB0kThLCIAUAEB6jkKmAEBGAYCdmCZTAKAEAGDLY2LjAFAtAGAnf+bTAICd+Jl7AQBblCEVAaCRACATZYhEAGg7AKzPVopFAFgwABRmS8Q5ANgtADBJV2ZIALC3AMDOEAuyAAgMADBRiIUpAAR7AGDIIyN4AISZABRG8lc88SuuEOcqAAB4mbI8uSQ5RYFbCC1xB1dXLh4ozkkXKxQ2YQJhmkAuwnmZGTKBNA/g88wAAKCRFRHgg/P9eM4Ors7ONo62Dl8t6r8G/yJiYuP+5c+rcEAAAOF0ftH+LC+zGoA7BoBt/qIl7gRoXgugdfeLZrIPQLUAoOnaV/Nw+H48PEWhkLnZ2eXk5NhKxEJbYcpXff5nwl/AV/1s+X48/Pf14L7iJIEyXYFHBPjgwsz0TKUcz5IJhGLc5o9H/LcL//wd0yLESWK5WCoU41EScY5EmozzMqUiiUKSKcUl0v9k4t8s+wM+3zUAsGo+AXuRLahdYwP2SycQWHTA4vcAAPK7b8HUKAgDgGiD4c93/+8//UegJQCAZkmScQAAXkQkLlTKsz/HCAAARKCBKrBBG/TBGCzABhzBBdzBC/xgNoRCJMTCQhBCCmSAHHJgKayCQiiGzbAdKmAv1EAdNMBRaIaTcA4uwlW4Dj1wD/phCJ7BKLyBCQRByAgTYSHaiAFiilgjjggXmYX4IcFIBBKLJCDJiBRRIkuRNUgxUopUIFVIHfI9cgI5h1xGupE7yAAygvyGvEcxlIGyUT3UDLVDuag3GoRGogvQZHQxmo8WoJvQcrQaPYw2oefQq2gP2o8+Q8cwwOgYBzPEbDAuxsNCsTgsCZNjy7EirAyrxhqwVqwDu4n1Y8+xdwQSgUXACTYEd0IgYR5BSFhMWE7YSKggHCQ0EdoJNwkDhFHCJyKTqEu0JroR+cQYYjIxh1hILCPWEo8TLxB7iEPENyQSiUMyJ7mQAkmxpFTSEtJG0m5SI+ksqZs0SBojk8naZGuyBzmULCAryIXkneTD5DPkG+Qh8lsKnWJAcaT4U+IoUspqShnlEOU05QZlmDJBVaOaUt2ooVQRNY9aQq2htlKvUYeoEzR1mjnNgxZJS6WtopXTGmgXaPdpr+h0uhHdlR5Ol9BX0svpR+iX6AP0dwwNhhWDx4hnKBmbGAcYZxl3GK+YTKYZ04sZx1QwNzHrmOeZD5lvVVgqtip8FZHKCpVKlSaVGyovVKmqpqreqgtV81XLVI+pXlN9rkZVM1PjqQnUlqtVqp1Q61MbU2epO6iHqmeob1Q/pH5Z/YkGWcNMw09DpFGgsV/jvMYgC2MZs3gsIWsNq4Z1gTXEJrHN2Xx2KruY/R27iz2qqaE5QzNKM1ezUvOUZj8H45hx+Jx0TgnnKKeX836K3hTvKeIpG6Y0TLkxZVxrqpaXllirSKtRq0frvTau7aedpr1Fu1n7gQ5Bx0onXCdHZ4/OBZ3nU9lT3acKpxZNPTr1ri6qa6UbobtEd79up+6Ynr5egJ5Mb6feeb3n+hx9L/1U/W36p/VHDFgGswwkBtsMzhg8xTVxbzwdL8fb8VFDXcNAQ6VhlWGX4YSRudE8o9VGjUYPjGnGXOMk423GbcajJgYmISZLTepN7ppSTbmmKaY7TDtMx83MzaLN1pk1mz0x1zLnm+eb15vft2BaeFostqi2uGVJsuRaplnutrxuhVo5WaVYVVpds0atna0l1rutu6cRp7lOk06rntZnw7Dxtsm2qbcZsOXYBtuutm22fWFnYhdnt8Wuw+6TvZN9un2N/T0HDYfZDqsdWh1+c7RyFDpWOt6azpzuP33F9JbpL2dYzxDP2DPjthPLKcRpnVOb00dnF2e5c4PziIuJS4LLLpc+Lpsbxt3IveRKdPVxXeF60vWdm7Obwu2o26/uNu5p7ofcn8w0nymeWTNz0MPIQ+BR5dE/C5+VMGvfrH5PQ0+BZ7XnIy9jL5FXrdewt6V3qvdh7xc+9j5yn+M+4zw33jLeWV/MN8C3yLfLT8Nvnl+F30N/I/9k/3r/0QCngCUBZwOJgUGBWwL7+Hp8Ib+OPzrbZfay2e1BjKC5QRVBj4KtguXBrSFoyOyQrSH355jOkc5pDoVQfujW0Adh5mGLw34MJ4WHhVeGP45wiFga0TGXNXfR3ENz30T6RJZE3ptnMU85ry1KNSo+qi5qPNo3ujS6P8YuZlnM1VidWElsSxw5LiquNm5svt/87fOH4p3iC+N7F5gvyF1weaHOwvSFpxapLhIsOpZATIhOOJTwQRAqqBaMJfITdyWOCnnCHcJnIi/RNtGI2ENcKh5O8kgqTXqS7JG8NXkkxTOlLOW5hCepkLxMDUzdmzqeFpp2IG0yPTq9MYOSkZBxQqohTZO2Z+pn5mZ2y6xlhbL+xW6Lty8elQfJa7OQrAVZLQq2QqboVFoo1yoHsmdlV2a/zYnKOZarnivN7cyzytuQN5zvn//tEsIS4ZK2pYZLVy0dWOa9rGo5sjxxedsK4xUFK4ZWBqw8uIq2Km3VT6vtV5eufr0mek1rgV7ByoLBtQFr6wtVCuWFfevc1+1dT1gvWd+1YfqGnRs+FYmKrhTbF5cVf9go3HjlG4dvyr+Z3JS0qavEuWTPZtJm6ebeLZ5bDpaql+aXDm4N2dq0Dd9WtO319kXbL5fNKNu7g7ZDuaO/PLi8ZafJzs07P1SkVPRU+lQ27tLdtWHX+G7R7ht7vPY07NXbW7z3/T7JvttVAVVN1WbVZftJ+7P3P66Jqun4lvttXa1ObXHtxwPSA/0HIw6217nU1R3SPVRSj9Yr60cOxx++/p3vdy0NNg1VjZzG4iNwRHnk6fcJ3/ceDTradox7rOEH0x92HWcdL2pCmvKaRptTmvtbYlu6T8w+0dbq3nr8R9sfD5w0PFl5SvNUyWna6YLTk2fyz4ydlZ19fi753GDborZ752PO32oPb++6EHTh0kX/i+c7vDvOXPK4dPKy2+UTV7hXmq86X23qdOo8/pPTT8e7nLuarrlca7nuer21e2b36RueN87d9L158Rb/1tWeOT3dvfN6b/fF9/XfFt1+cif9zsu72Xcn7q28T7xf9EDtQdlD3YfVP1v+3Njv3H9qwHeg89HcR/cGhYPP/pH1jw9DBY+Zj8uGDYbrnjg+OTniP3L96fynQ89kzyaeF/6i/suuFxYvfvjV69fO0ZjRoZfyl5O/bXyl/erA6xmv28bCxh6+yXgzMV70VvvtwXfcdx3vo98PT+R8IH8o/2j5sfVT0Kf7kxmTk/8EA5jz/GMzLdsAAAAgY0hSTQAAeiUAAICDAAD5/wAAgOkAAHUwAADqYAAAOpgAABdvkl/FRgAABONJREFUeNrEmMFvG0UUh7+13dI0Ng0pVEJIEJCQcgmEI1zo7pEDyh+A1JY7EhUnTglIvSG1cEGIQ3JBAg5VwglBWW9JSQWFkoCsxFjJOgpWtlXjNE6dOl57h8vbauV61/baEU8aRfaMZ7/83pvfzKymlCIqDMOYBM4Bk8DZNkMs4DowBxSj5jJNk15CC4MzDOMsMB0CFBYWcBFYHgRcIgTsMpDtEQwZ/ycwwwAi1QI1IlCTfc47DbwAXOhnklblBgHmx3lgdiBwkspBgQUB34/7Y00p5Rd/tovxy1L0e8ApYAoY6+J3LwLFXhdEKlAjnVbhhTZWcVEWQSfVp+PUX0J8LGpVzpmmqZumWYwAf018Liq9Y3Fq7lxE/7xpmt3+xxfC/E1iKg5clGoXe5wvavybceAmI9JZ7HE+K0K9sdhW0iZWYjqAFfL95CDhlmPC7Q3KJKPgxvifIwru1ZhzhhV+MQ7c/TBvkoNALzEWsfpjwYXV1kiMffFyRF9R07SE9ngQ1hIdCn/aMIzzYZ3ZbFaTllBKvRtltJ7n5YDjwBPSjsv2mRKRtHZ76/UOCs0ahjFmmuZMEEomTExMTIyOjo5+omnaO1GSViqVW0AaUIEG0AQa0pqA5/dpuq6PALtdpKwIzHuet9hsNveVUqeTyeTbyWTyLTmhhIZSasuyrNcD6mgCoAlQE6gDh9I8QPlHpjhH8q6j0Wh8s7i4+AFwTBRPtaTRA1ygCjzwAX0rWThKv2o2mwvAAfBQFEsBQ8BJaWlR/0n5PgloPtzcEbIVl5aWvhVFHggksihOAsOBlpbvE49M2DTN+8D8EcHN67ruF71fU0og0oE2HADTWneIT48ILjivJik90aKYD6YFVq1KBC68VhwX76QaUBTrSYlCzwBPi8n7qp0QNatATeAe21s/GiSZUuqzbDZ7TGrrNPA88BLwHPAUkJE+gH3ZSmuPfK71dYRhGPYgTiRKqUXLsqbk4aeAM8CzAumvyIZAbQHrQEnU8x678QfUm+0XznGcr4BXBGxUlEoHvM4H2wX+Be4ErCb8RU6/6tVqtX9u3rz5uSg0FNhPE/JwV1K4CeQBWz43gnCJkJR83I9qtm2vAuOB+jojBjssyj2UFOZlEe61goXCWZY1p5S6EQdsZ2en6DhOXWprRKDSUnuaKFQA/gY2JK1uK1jkSbher1+KsU256+vrm7IK0/LX97AG4AA5eU223i6VHeGUUmppaSnruu7VXuC2t7e3q9VqMuD4Q6JWRdS6Bfwhqaz4ZhvnDtGwbftDpVS1G7CDg4OHhUJhR6BOymHSBe7KNfMX4LbYRrUTWCc4VSqVnN3d3SvdwBUKhXuBlalJkeeBG3Kg/QvYlo3f6+v2pZTygNrKyspsrVbLR01SKpX2y+WyJ75ZE4u4BfwE/CyQ5bDCj6McUqxl27ZnPM87bDfg8PCwadv2gTz4jqTwR+B74FcB3dd1vdELWEc4Ua/qOM5vjuN83W7M2tranuu6O8CavIBcAK6JVdwFDnVd9+LYUqqbUzZwL5/Pf5nJZN7IZDIv+x2bm5uVcrmcl3q6LarZUm9uXKhu0+qrdwDYq6url+r1elVWZ21jY+Ma8B1wVdTKATtAvV+wbpXzr2+71Wr190Kh8MX4+Ph7uVxuAfhBfGtLjuCuruuKAcV/AwDnrxMM7gFGVQAAAABJRU5ErkJggg==',
			position: latLang,
			map: map,
			title: ""
		});

		marker.setMap(map);
		google.maps.event.addListener(marker, "click", function() {
			// Add optionally an action for when the marker is clicked
		});

		// kepp googlemap responsive - center on resize
		google.maps.event.addDomListener(window, 'resize', function() {
			map.setCenter(latLang);
		});

	}

	
	function showMap(initWhat) {
		var script 		= document.createElement('script');
		script.type 	= 'text/javascript';
		script.src 		= 'https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=true&callback='+initWhat;
		document.body.appendChild(script);
	}

	
	// INIT CONTACT, NLY IF #contactMap EXISRS
	if(jQuery("#gmap").length > 0) {
		showMap('contactMap');
	}



/**	Nicescroll
 **************************************************************** **/
	function _niceScroll() {
		if(jQuery().niceScroll) {
			jQuery(".nicescroll").niceScroll({
				// background:"#ccc",
				scrollspeed:60,
				mousescrollstep:35,
				cursorborder:0,
				cursorcolor:"rgba(0,0,0,.6)",
				horizrailenabled:false,
				zindex:99999,
				autohidemode:false,
				cursorwidth:8
			});
		}
	}



/**	Lazyload
	[<img class="lazy" src="assets/images/lazy.png" data-original="image.jpg" alt="" />]
*************************************************** **/
	function _lazyImage() {
		if(jQuery().lazyload) {
			jQuery("img.lazy").lazyload({effect:"fadeIn"});
		};
	}
	

	

/** **************************************************************************************************************** **/
/** **************************************************************************************************************** **/
/** **************************************************************************************************************** **/
/** **************************************************************************************************************** **/






/** Misc
 **************************************************************** **/
	// scroll 
	function wheel(e) {
	  e.preventDefault();
	}

	function disable_scroll() {
	  if (window.addEventListener) {
		  window.addEventListener('DOMMouseScroll', wheel, false);
	  }
	  window.onmousewheel = document.onmousewheel = wheel;
	}

	function enable_scroll() {
		if (window.removeEventListener) {
			window.removeEventListener('DOMMouseScroll', wheel, false);
		}
		window.onmousewheel = document.onmousewheel = document.onkeydown = null;  
	}





/** COUNT TO
	https://github.com/mhuggins/jquery-countTo
 **************************************************************** **/
	(function ($) {
		$.fn.countTo = function (options) {
			options = options || {};

			return jQuery(this).each(function () {
				// set options for current element
				var settings = jQuery.extend({}, $.fn.countTo.defaults, {
					from:            jQuery(this).data('from'),
					to:              jQuery(this).data('to'),
					speed:           jQuery(this).data('speed'),
					refreshInterval: jQuery(this).data('refresh-interval'),
					decimals:        jQuery(this).data('decimals')
				}, options);

				// how many times to update the value, and how much to increment the value on each update
				var loops = Math.ceil(settings.speed / settings.refreshInterval),
					increment = (settings.to - settings.from) / loops;

				// references & variables that will change with each update
				var self = this,
					$self = jQuery(this),
					loopCount = 0,
					value = settings.from,
					data = $self.data('countTo') || {};

				$self.data('countTo', data);

				// if an existing interval can be found, clear it first
				if (data.interval) {
					clearInterval(data.interval);
				}
				data.interval = setInterval(updateTimer, settings.refreshInterval);

				// __construct the element with the starting value
				render(value);

				function updateTimer() {
					value += increment;
					loopCount++;

					render(value);

					if (typeof(settings.onUpdate) == 'function') {
						settings.onUpdate.call(self, value);
					}

					if (loopCount >= loops) {
						// remove the interval
						$self.removeData('countTo');
						clearInterval(data.interval);
						value = settings.to;

						if (typeof(settings.onComplete) == 'function') {
							settings.onComplete.call(self, value);
						}
					}
				}

				function render(value) {
					var formattedValue = settings.formatter.call(self, value, settings);
					$self.html(formattedValue);
				}
			});
		};

		$.fn.countTo.defaults = {
			from: 0,               // the number the element should start at
			to: 0,                 // the number the element should end at
			speed: 1000,           // how long it should take to count between the target numbers
			refreshInterval: 100,  // how often the element should be updated
			decimals: 0,           // the number of decimal places to show
			formatter: formatter,  // handler for formatting the value before rendering
			onUpdate: null,        // callback method for every time the element is updated
			onComplete: null       // callback method for when the element finishes updating
		};

		function formatter(value, settings) {
			return value.toFixed(settings.decimals);
		}
	}(jQuery));





/** FITVIDS
	http://fitvidsjs.com/
 **************************************************************** **/
	(function( $ ){

	  "use strict";

	  $.fn.fitVids = function( options ) {
		var settings = {
		  customSelector: null
		};

		if(!document.getElementById('fit-vids-style')) {

		  var div = document.createElement('div'),
			  ref = document.getElementsByTagName('base')[0] || document.getElementsByTagName('script')[0];

		  div.className = 'fit-vids-style';
		  div.id = 'fit-vids-style';
		  div.style.display = 'none';
		  div.innerHTML = '&shy;<style>         \
			.fluid-width-video-wrapper {        \
			   width: 100%;                     \
			   position: relative;              \
			   padding: 0;                      \
			}                                   \
												\
			.fluid-width-video-wrapper iframe,  \
			.fluid-width-video-wrapper object,  \
			.fluid-width-video-wrapper embed {  \
			   position: absolute;              \
			   top: 0;                          \
			   left: 0;                         \
			   width: 100%;                     \
			   height: 100%;                    \
			}                                   \
		  </style>';

		  ref.parentNode.insertBefore(div,ref);

		}

		if ( options ) {
		  jQuery.extend( settings, options );
		}

		return this.each(function(){
		  var selectors = [
			"iframe[src*='player.vimeo.com']",
			"iframe[src*='youtube.com']",
			"iframe[src*='youtube-nocookie.com']",
			"iframe[src*='kickstarter.com'][src*='video.html']",
			"object",
			"embed"
		  ];

		  if (settings.customSelector) {
			selectors.push(settings.customSelector);
		  }

		  var $allVideos = jQuery(this).find(selectors.join(','));
		  $allVideos = $allVideos.not("object object"); // SwfObj conflict patch

		  $allVideos.each(function(){
			var $_t = jQuery(this);
			if (this.tagName.toLowerCase() === 'embed' && $_t.parent('object').length || $_t.parent('.fluid-width-video-wrapper').length) { return; }
			var height = ( this.tagName.toLowerCase() === 'object' || ($_t.attr('height') && !isNaN(parseInt($_t.attr('height'), 10))) ) ? parseInt($_t.attr('height'), 10): $_t.height(),
				width = !isNaN(parseInt($_t.attr('width'), 10)) ? parseInt($_t.attr('width'), 10): $_t.width(),
				aspectRatio = height / width;
			if(!$_t.attr('id')){
			  var videoID = 'fitvid' + Math.floor(Math.random()*999999);
			  $_t.attr('id', videoID);
			}
			$_t.wrap('<div class="fluid-width-video-wrapper"></div>').parent('.fluid-width-video-wrapper').css('padding-top', (aspectRatio * 100)+"%");
			$_t.removeAttr('height').removeAttr('width');
		  });
		});
	  };
	})(jQuery);

	// remove fitvids for a specific element: jQuery("#myDiv").unFitVids();
	jQuery.fn.unFitVids = function () {
		var id = jQuery(this).attr("id");
		var $children = jQuery("#" + id + " .fluid-width-video-wrapper").children().clone();
		jQuery("#" + id + " .fluid-width-video-wrapper").remove(); //removes the element
		jQuery("#" + id).append($children); //adds it to the parent
	};




/** WAIT FOR IMAGES [used by masonry]
	https://github.com/alexanderdickson/waitForImages
 **************************************************************** **/
	;(function ($) {
		// Namespace all events.
		var eventNamespace = 'waitForImages';

		// CSS properties which contain references to images.
		$.waitForImages = {
			hasImageProperties: ['backgroundImage', 'listStyleImage', 'borderImage', 'borderCornerImage', 'cursor']
		};

		// Custom selector to find `img` elements that have a valid `src` attribute and have not already loaded.
		$.expr[':'].uncached = function (obj) {
			// Ensure we are dealing with an `img` element with a valid `src` attribute.
			if (!$(obj).is('img[src!=""]')) {
				return false;
			}

			// Firefox's `complete` property will always be `true` even if the image has not been downloaded.
			// Doing it this way works in Firefox.
			var img = new Image();
			img.src = obj.src;
			return !img.complete;
		};

		$.fn.waitForImages = function (finishedCallback, eachCallback, waitForAll) {

			var allImgsLength = 0;
			var allImgsLoaded = 0;

			// Handle options object.
			if ($.isPlainObject(arguments[0])) {
				waitForAll = arguments[0].waitForAll;
				eachCallback = arguments[0].each;
				// This must be last as arguments[0]
				// is aliased with finishedCallback.
				finishedCallback = arguments[0].finished;
			}

			// Handle missing callbacks.
			finishedCallback = finishedCallback || $.noop;
			eachCallback = eachCallback || $.noop;

			// Convert waitForAll to Boolean
			waitForAll = !! waitForAll;

			// Ensure callbacks are functions.
			if (!$.isFunction(finishedCallback) || !$.isFunction(eachCallback)) {
				throw new TypeError('An invalid callback was supplied.');
			}

			return this.each(function () {
				// Build a list of all imgs, dependent on what images will be considered.
				var obj = $(this);
				var allImgs = [];
				// CSS properties which may contain an image.
				var hasImgProperties = $.waitForImages.hasImageProperties || [];
				// To match `url()` references.
				// Spec: http://www.w3.org/TR/CSS2/syndata.html#value-def-uri
				var matchUrl = /url\(\s*(['"]?)(.*?)\1\s*\)/g;

				if (waitForAll) {

					// Get all elements (including the original), as any one of them could have a background image.
					obj.find('*').addBack().each(function () {
						var element = $(this);

						// If an `img` element, add it. But keep iterating in case it has a background image too.
						if (element.is('img:uncached')) {
							allImgs.push({
								src: element.attr('src'),
								element: element[0]
							});
						}

						$.each(hasImgProperties, function (i, property) {
							var propertyValue = element.css(property);
							var match;

							// If it doesn't contain this property, skip.
							if (!propertyValue) {
								return true;
							}

							// Get all url() of this element.
							while (match = matchUrl.exec(propertyValue)) {
								allImgs.push({
									src: match[2],
									element: element[0]
								});
							}
						});
					});
				} else {
					// For images only, the task is simpler.
					obj.find('img:uncached')
						.each(function () {
						allImgs.push({
							src: this.src,
							element: this
						});
					});
				}

				allImgsLength = allImgs.length;
				allImgsLoaded = 0;

				// If no images found, don't bother.
				if (allImgsLength === 0) {
					finishedCallback.call(obj[0]);
				}

				$.each(allImgs, function (i, img) {

					var image = new Image();

					// Handle the image loading and error with the same callback.
					$(image).on('load.' + eventNamespace + ' error.' + eventNamespace, function (event) {
						allImgsLoaded++;

						// If an error occurred with loading the image, set the third argument accordingly.
						eachCallback.call(img.element, allImgsLoaded, allImgsLength, event.type == 'load');

						if (allImgsLoaded == allImgsLength) {
							finishedCallback.call(obj[0]);
							return false;
						}

					});

					image.src = img.src;
				});
			});
		};
	}(jQuery));




/** LAZYLOAD
	http://www.appelsiini.net/projects/lazyload
 **************************************************************** **/
	(function (c, b, a, e) { var d = c(b); c.fn.lazyload = function (f) { var h = this; var i; var g = { threshold: 0, failure_limit: 0, event: "scroll", effect: "show", container: b, data_attribute: "original", skip_invisible: true, appear: null, load: null, placeholder: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAANSURBVBhXYzh8+PB/AAffA0nNPuCLAAAAAElFTkSuQmCC" }; function j() { var k = 0; h.each(function () { var l = c(this); if (g.skip_invisible && !l.is(":visible")) { return } if (c.abovethetop(this, g) || c.leftofbegin(this, g)) { } else { if (!c.belowthefold(this, g) && !c.rightoffold(this, g)) { l.trigger("appear"); k = 0 } else { if (++k > g.failure_limit) { return false } } } }) } if (f) { if (e !== f.failurelimit) { f.failure_limit = f.failurelimit; delete f.failurelimit } if (e !== f.effectspeed) { f.effect_speed = f.effectspeed; delete f.effectspeed } c.extend(g, f) } i = (g.container === e || g.container === b) ? d : c(g.container); if (0 === g.event.indexOf("scroll")) { i.bind(g.event, function () { return j() }) } this.each(function () { var k = this; var l = c(k); k.loaded = false; if (l.attr("src") === e || l.attr("src") === false) { l.attr("src", g.placeholder) } l.one("appear", function () { if (!this.loaded) { if (g.appear) { var m = h.length; g.appear.call(k, m, g) } c("<img />").bind("load", function () { var o = l.data(g.data_attribute); l.hide(); if (l.is("img")) { l.attr("src", o) } else { l.css("background-image", "url('" + o + "')") } l[g.effect](g.effect_speed); k.loaded = true; var n = c.grep(h, function (q) { return !q.loaded }); h = c(n); if (g.load) { var p = h.length; g.load.call(k, p, g) } }).attr("src", l.data(g.data_attribute)) } }); if (0 !== g.event.indexOf("scroll")) { l.bind(g.event, function () { if (!k.loaded) { l.trigger("appear") } }) } }); d.bind("resize", function () { j() }); if ((/iphone|ipod|ipad.*os 5/gi).test(navigator.appVersion)) { d.bind("pageshow", function (k) { if (k.originalEvent && k.originalEvent.persisted) { h.each(function () { c(this).trigger("appear") }) } }) } c(a).ready(function () { j() }); return this }; c.belowthefold = function (g, h) { var f; if (h.container === e || h.container === b) { f = (b.innerHeight ? b.innerHeight : d.height()) + d.scrollTop() } else { f = c(h.container).offset().top + c(h.container).height() } return f <= c(g).offset().top - h.threshold }; c.rightoffold = function (g, h) { var f; if (h.container === e || h.container === b) { f = d.width() + d.scrollLeft() } else { f = c(h.container).offset().left + c(h.container).width() } return f <= c(g).offset().left - h.threshold }; c.abovethetop = function (g, h) { var f; if (h.container === e || h.container === b) { f = d.scrollTop() } else { f = c(h.container).offset().top } return f >= c(g).offset().top + h.threshold + c(g).height() }; c.leftofbegin = function (g, h) { var f; if (h.container === e || h.container === b) { f = d.scrollLeft() } else { f = c(h.container).offset().left } return f >= c(g).offset().left + h.threshold + c(g).width() }; c.inviewport = function (f, g) { return !c.rightoffold(f, g) && !c.leftofbegin(f, g) && !c.belowthefold(f, g) && !c.abovethetop(f, g) }; c.extend(c.expr[":"], { "below-the-fold": function (f) { return c.belowthefold(f, { threshold: 0 }) }, "above-the-top": function (f) { return !c.belowthefold(f, { threshold: 0 }) }, "right-of-screen": function (f) { return c.rightoffold(f, { threshold: 0 }) }, "left-of-screen": function (f) { return !c.rightoffold(f, { threshold: 0 }) }, "in-viewport": function (f) { return c.inviewport(f, { threshold: 0 }) }, "above-the-fold": function (f) { return !c.belowthefold(f, { threshold: 0 }) }, "right-of-fold": function (f) { return c.rightoffold(f, { threshold: 0 }) }, "left-of-fold": function (f) { return !c.rightoffold(f, { threshold: 0 }) } }) })(jQuery, window, document);

	/*
     *  jodstrap
     *  Copyrights - saltflakes.net
     *  Creator: saltflakes.net
     *  January 2013
     */


	$(document).ready(function () {
	    //isotope filter portfolio2:
	    $(".sf-da-isotope").isotope({
	        //options
	        itemSelector: '.sf-da-item',
	        layoutMode: 'fitRows',
	    });

	    // filter items when filter link is clicked:
	    $('#da-filters a').click(function () {
	        var selector = $(this).attr('data-filter');
	        $(".sf-da-isotope").isotope({
	            filter: selector
	        });
	        $('.sf-da-isotope-filters li').removeClass("active");
	        $(this).closest('li').addClass("active");
	        return false;
	    });
	    $(' #da-thumbs > li ').each(function () {
	        $(this).hoverdir();
	    });

	    //isotope filter options:
	    $(".sf-isotope").isotope({
	        itemSelector: '.sf-item',
	        layoutMode: 'fitRows',
	    });
	    $(window).resize();
	    var $container = $('.sf-isotope')
	    $container.isotope({
	        // options...
	        resizable: false, // disable normal resizing
	        masonry: {
	            columnWidth: $container.width() / 2
	        }
	    });
	    // isotope - update columnWidth on window resize
	    $(window).smartresize(function () {
	        $container.isotope({
	            // update columnWidth to a percentage of container width
	            masonry: {
	                columnWidth: $container.width() / 4
	            }
	        });
	    });

	    // isotope - filter items when filter link is clicked:
	    $('#filters a').click(function () {
	        var selector = $(this).attr('data-filter');
	        $(".sf-isotope").isotope({
	            filter: selector
	        });
	        $('.sf-isotope-filters li').removeClass("active");
	        $(this).closest('li').addClass("active");
	        return false;
	    });

	    // portfolio sf item hover:
	    $(".sf-item").hover(function () {
	        $(this).children().children('.sf-item-overlay').stop(true, true).animate({
	            'opacity': '1'
	        }, 400);
	        $(this).children().children('.sf-item-text').stop(true, true).animate({
	            'color': '#FFFFFF'
	        }, 400);
	    }, function () {
	        $(this).children().children('.sf-item-overlay').stop(true, true).animate({
	            'opacity': '0'
	        }, 400);
	        $(this).children().children('.sf-item-text').stop(true, true).animate({
	            'color': '#222222'
	        }, 400);
	    });


	});

