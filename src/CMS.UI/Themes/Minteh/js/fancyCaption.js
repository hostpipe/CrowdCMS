/*
 * fancyCaption
 *	by Alex Schornberg
 *	v2.0
 *	http://herz-as.net
 */

	    $.fn.fancyCaption = function(settings) {
		settings = $.extend({
		    slideTopBar: '.slide-top',	    // class for top bar caption. set to false to disable top bar slide
		    slideLeftBar: '.slide-left',    // class for top bar caption. set to false to disable top bar slide
		    slideBottomBar: '.slide-bottom',// class for bottom bar caption. set to false to disable bottom bar slide
		    slideRightBar: '.slide-right',  // class for bottom bar caption. set to false to disable bottom bar slide
		    slideTimeIn: 500,		    // time in ms to slide in top/bottom captions
		    slideEasingIn: 'swing',	    // slide in easing
		    slideTimeOut: 500,		    // time in ms to slide out top/bottom captions
		    slideEasingOut: 'swing',	    // slide out easing

		    fadeElement: '.fade',	    // class of the fade element
		    fadeTimeIn: 500,		    // time in ms to fade in overlay container
		    fadeEasingIn: 'swing',	    // fade in easing
		    fadeTimeOut: 500,		    // time in ms to fade out overlay container
		    fadeEasingOut: 'swing',	    // fade out easing
		    fadeFrom: 1,		    // final opacity the overlay container fades to (0=none, 1=full transparency)
		    fadeTo: 0.5			    // final opacity the overlay container fades to (0=none, 1=full transparency)
		}, settings);

		return this.each( function(){
		    if( settings.fadeElement ) $(this).find(settings.fadeElement).stop().animate({ opacity: settings.fadeFrom }, 0 ).nextAll();

		    if( settings.slideTopBar ) $(this).find(settings.slideTopBar).css('top', '-'+$(this).find(settings.slideTopBar).outerHeight()+'px');
		    if( settings.slideRightBar ) $(this).find(settings.slideRightBar).css('right', '-'+$(this).find(settings.slideRightBar).outerWidth()+'px');
		    if( settings.slideBottomBar ) $(this).find(settings.slideBottomBar).css('bottom', '-'+$(this).find(settings.slideBottomBar).outerHeight()+'px');
		    if( settings.slideLeftBar ) $(this).find(settings.slideLeftBar).css('left', '-'+$(this).find(settings.slideLeftBar).outerWidth()+'px');

		    if( settings.slideBottomBar || settings.slideTopBar || settings.slideRightBar || settings.slideLeftBar || settings.fadeElement ){
			$(this).hover(
			    function () {
				if( settings.fadeElement ) $(this).find(settings.fadeElement).first().stop().animate({ opacity: settings.fadeTo }, settings.fadeTimeIn, settings.fadeEasingIn );
				if( settings.slideTopBar ) $(this).find(settings.slideTopBar).stop().show().animate({ top: '0' }, settings.slideTimeIn, settings.slideEasingIn);
				if( settings.slideRightBar ) $(this).find(settings.slideRightBar).stop().show().animate({ right: '0' }, settings.slideTimeIn, settings.slideEasingIn);
				if( settings.slideBottomBar ) $(this).find(settings.slideBottomBar).stop().show().animate({ bottom: '0' }, settings.slideTimeIn, settings.slideEasingIn);
				if( settings.slideLeftBar ) $(this).find(settings.slideLeftBar).stop().show().animate({ left: '0' }, settings.slideTimeIn, settings.slideEasingIn);
			    },
			    function () {
				if( settings.fadeElement ) $(this).find(settings.fadeElement).first().stop().animate({ opacity: settings.fadeFrom }, settings.fadeTimeOut, settings.fadeEasingOut);
				if( settings.slideTopBar ) $(this).find(settings.slideTopBar).stop().animate({ top: '-'+$(this).find(settings.slideTopBar).outerHeight(), }, settings.slideTimeOut, settings.slideEasingOut, function(){ $(this).children(settings.slideTopBar).hide() });
				if( settings.slideRightBar ) $(this).find(settings.slideRightBar).stop().show().animate({ right: '-'+$(this).find(settings.slideRightBar).outerWidth() }, settings.slideTimeIn, settings.slideEasingIn);
				if( settings.slideBottomBar ) $(this).find(settings.slideBottomBar).stop().animate({ bottom: '-'+$(this).find(settings.slideBottomBar).outerHeight(), }, settings.slideTimeOut, settings.slideEasingOut, function(){ $(this).children(settings.slideBottomBar).hide() });
				if( settings.slideLeftBar ) $(this).find(settings.slideLeftBar).stop().show().animate({ left: '-'+$(this).find(settings.slideLeftBar).outerWidth() }, settings.slideTimeIn, settings.slideEasingIn);
			    }
			);
		    }
		});
	    };