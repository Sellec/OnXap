//Global Options 
$(function () {
    $("a[name='modal']").click(function (e) { e.preventDefault(); var id = $(this).attr('href'); open_modal(id); });
    $('.window .close').click(function (e) { e.preventDefault(); close_modal() });
    $('#mask,.close_form').click(function () { close_modal(); });
    $('#mod_error').click(function () { $(this).fadeOut(); $('#item_order').animate({ 'opacity': 1 }) })

    try { $("#gall_photo a,.ontop,#articles_preview a,.popup,.pages_preview a,.onlightbox,#item_photo a").fancybox(); } catch (err) { }

    var aleft = 0;
    $(".pages a").each(function () {
        var img = $(this).find("img");
        if (img.length > 0) {
            $(this).fancybox();
            if (img.attr("align") == "left") img.addClass("ileft");
            if (img.attr("align") == "right") img.addClass("iright");
        }
    })

    $(".info_table td").hover(function () { $(this).parent().find("td").addClass("hovered") }, function () { $(this).parent().find("td").removeClass("hovered") })
    $(".both tr").each(function () { $(this).find("td:first").addClass("first_td"); })

    $(".item_popup").click(function () {
        open_popup($(this).attr("href"), "Просмотр продукции", null);
        var par = $(this).parent().parent("li");
        if (par.length) {
            var str = par.find("a:first").attr("name").replace(/prd_/, '');
            window.location.hash = "#" + str;
        }
        //alert($(this).parent("li").find("a:first").attr("name"))
        return false;
    })

    if (window.location.hash != '') {
        jQuery.scrollTo($("a[name='prd_" + window.location.hash.replace('#', '') + "']"), 800);
        open_popup('/products/item/' + window.location.hash.replace('#', ''), "Просмотр продукции", null);
    }

    var fields = 0;
    $(".field_add").click(function () {
        fields++;
        if (fields <= 5) $(this).before("<input type='file' size='25' name='file_upload[]' class='file_upload' /><br />");
        if (fields == 5) $(this).hide();
        return false;
    })
});

function open_modal(id,message){
	if (message) $(id).html(message);
	var maskHeight = $(document).height();
	var maskWidth = $(window).width();
	$('#mask').css({'width':maskWidth,'height':maskHeight,'opacity':0.5});
	$('#mask').fadeIn(500);            
	var winH = $(window).height()/2+$(window).scrollTop();
	var winW = $(window).width();

	//$(id).css('top', winH-$(id).height()/2);
	//$(id).css('left', winW/2-$(id).width()/2);
	$(id).find(".close").css({'top':winH-$(id).height()/2 + 5,'left':winW/2+$(id).width()/2-5})
	$(id).fadeIn(500);
	return false;
}
function close_modal(){
	$('#mask').fadeOut("slow");
	$('.window').fadeOut("slow");
	return false;
}

function nl2br (str)
{
	return str.replace(/([^>])\n/g, '$1<br/>');
}
