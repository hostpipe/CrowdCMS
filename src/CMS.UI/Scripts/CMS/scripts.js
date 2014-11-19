var FE;
$(document).ready(function () {
    //External Links
    externalLinks();
    var container = document.getElementById('homepage');
    if (container != null) {
        var msnry = new Masonry(container, {
            // options
            itemSelector: '.item'
        });
    }
});

function externalLinks() {
    if (!document.getElementsByTagName) return;
    var anchors = document.getElementsByTagName("a");
    for (var i = 0; i < anchors.length; i++) {
        var anchor = anchors[i];
        if (anchor.getAttribute("href") &&
			(anchor.getAttribute("rel") == "external" || anchor.getAttribute("rel") == "external nofollow"))
            anchor.target = "_blank";
    }
}
