function getResultAnim(form, div) {
    form.submit(function () {
        var div_res = div;
        $("#loading_img").fadeIn("slow");
        div_res.fadeIn("slow");
        setTimeout(function () { div_res.fadeOut("slow") }, 2500);
        var opros = setInterval(function () {
            if (div_res.text() != "") {
                setTimeout(function () { div_res.text(""); $("#loading_img").fadeOut("slow"); }, 3500);
                clearInterval(opros);
            };
        }, 1000);
        return false;
    });
}

function endAnim(div_res) {
    $("#loading_img").fadeOut();
    div_res.fadeIn("slow");
    $("#admin_botgr").fadeIn("slow");
    div_res.click(function () { $(this).fadeOut(); $("#admin_botgr").fadeOut(); });
}

function stAnim() {
    $("form").submit(function () {
        $("#action_result").fadeOut().removeClass("action_error");
        $("#loading_img").fadeIn("slow");
    });
    $("#loading_img").hide();
}

function stAnimPh() {
    $("form").submit(function () {
        $("#action_result").fadeOut();
        if ($(this).attr("id") == "form_add_photo") $("#loading_photo").fadeIn("slow");
    });
    $("#loading_photo").hide();
}
