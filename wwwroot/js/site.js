// Hello Microsoft Security Community :)

$(function () {

    $("#loader").hide();
    $("#csp_status").hide();

    if (document.documentMode || /Edge/.test(navigator.userAgent)) {
        $('.modal').removeClass('fade');
    }

    like = function (id) {
        $.get("/like/" + id, function () { });
    };

    fillIn = function (userInput) {

        // check if this is the hash
        if (userInput.startsWith('#')) {
            userInput = userInput.substring(1, userInput.length);
            $("#search").val(userInput);
        }
        else {
            location.hash = userInput;
        }

        // security fix, good luck hackers!
        var evilChars = /<.*>/;
        userInput = decodeURIComponent(userInput.replace(evilChars, ''));

        if (userInput.length === "") {
            $("#results").html("");
        }
        else {
            $("#loader").show();

            $.get("/list",
                { tla: userInput },
                function (resp) {
                    t = "";
                    if (resp.length > 0) {
                        j = $.parseJSON(resp);
                        // If we have a complete TLA
                        if (userInput.length > 2) {
                            if (j.success) {
                                j.results.forEach(function (obj) {
                                    t += "<div class=\"container-fluid\"><div class=\"row\">" +
                                        "  <div class=\"col-4 text-right\"><strong>" +
                                        obj[1] +
                                        "</strong></div>" +
                                        "  <div class=\"col-8 text-left\">" +
                                        obj[2] + " &nbsp;" + obj[4] +
                                        " <a title=\"Like this definition\" href=\"#" + obj[0] + "\" onclick=\"like(" + obj[5] + ")\">&#x1f44d;</a>" +
                                        (obj[4] === 1 ? "" : "<sup>'s</sup>") +
                                        "<div><sup style='color:#888'>Added by " + obj[3] + "</sup></div>" +
                                        "  </div>" +
                                        "</div > ";
                                });
                            }

                            t += "<br/><div>Add <a id=\"addlink\" data-toggle=\"modal\" data-target=\"#myModal\" href=\"#" + userInput + "\" data=\"" + userInput + "\">another</a> definition</div>";
                        }
                        else {
                            if (j.success && userInput.length > 0) {
                                userInput = userInput.toLowerCase();
                                j.results.forEach(function (obj) {
                                    styledTla = obj[0].toLowerCase().replace(userInput, "<strong>" + userInput + "</strong>").toUpperCase();
                                    styledTla = "<a href=\"#" + obj[0] + "\">" + styledTla + "</a>";
                                    t += "<div class=\"container-fluid\"><div class=\"row\">" +
                                        "  <div class=\"col-3\">" + styledTla + "</div>" +
                                        "  <div class=\"col-9\">" + obj[1] + "</div>" +
                                        "</div></div>";
                                });
                            }
                        }

                        $("#loader").hide();
                        $("#results").html(t);
                    }
                    else {

                        if (userInput.length > 0) {
                            err = "Could not find " + userInput + ", feel free to <a id=\"addlink\" data-toggle=\"modal\" data-target=\"#myModal\" href=\"#\" data=\"" + userInput + "\">add</a> it";
                            $("#results").html(err);
                        }
                        else {
                            $("#results").html("");
                        }

                        // The DOM has our element now
                        $("#addlink").on("click", function (e) {
                            e.preventDefault();
                            e.stopPropagation();

                            $("#myModal").modal({ backdrop: true }); // .('show');
                        });

                    }
                }
            );
        }
    };

    // When the modal is shown
    $("#myModal").on('shown.bs.modal', function () {
        $("#modalmsg").hide().removeClass("alert-danger").removeClass("alert-success");
        $("#addacronym").off("click");

        s = $("#addlink").attr("data");

        $("#title").val("");
        $("#desc").val("")

        if (s.length > 0) {
            $("#acronym").val(s);
        }

        $("#addacronym").on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            tla = $("#acronym").val();
            title = $("#title").val();
            desc = $("#desc").val();

            if (tla.length !== 3) {
                alert("A three letter acronym can only have 3 letters. :)");
                $("#acronym").focus();
                return false;
            }

            if (desc.length === 0) {
                alert("You need a description. ");
                $("#desc").focus();
                return false;
            }

            if (title.length === 0) {
                alert("You need a title. ");
                $("#title").focus();
                return false;
            }

            $.ajax({
                method: "POST",
                url: "/add",
                dataType: 'json',
                data: {
                    tla: tla,
                    title: title,
                    desc: desc
                },
                headers: {
                    "RequestVerificationToken": $("#xsrf").val()
                }
            })
                .done(function (resp) {
                    if (resp && resp.length > 0) {
                        j = $.parseJSON(resp);
                        if (j.status === 'success') {
                            $("#addacronym").off("click");
                            $("#modalmsg").addClass("alert-success").html("Added the acronym!").show();
                            location.hash = "";
                            location.hash = tla;

                            $('#myModal').delay(1000).fadeOut(450);

                            setTimeout(function () {
                                $('#myModal').modal("hide");
                            }, 1500);

                        }
                        else // We have an error
                        {
                            $("#modalmsg").addClass("alert-danger").html("<strong>Error:</strong> " + j.msg).show();
                            return false;
                        }
                    }
                })

                .fail(function (err) {
                    alert("Error happened: " + err);
                    return false;
                });
        });
    });


    // Hash has the # char in it, so let's not send that'
    if (location.hash !== null && location.hash.length > 1) {
        fillIn(location.hash);
    }

    $(window).on("hashchange", function () {
        fillIn(location.hash);
    });

    $("#search").on("keyup", function () {
        fillIn($("#search").val());
    });

    $('#myModal').on('shown.bs.modal', function () {
        $('#myInput').focus();
    });

    uploadFile = function () {
        // if (f.files && f.files[0]) {
        if (!false) {
            console.log("Got a file, uploading it");
            // fd.append("f", f.files[0]);
            var fd = new FormData();
            fd.append("f", $("#filetoupload").get(0).files[0]);
            fd.append("container", "/photos/");
            fd.append("RequestVerificationToken", $("#xsrf").val());

            $.ajax({
                url: "/uplo" + "ad",
                type: 'POST',
                contentType: false,
                processData: false,
                cache: false,
                data: fd,
                success: function (resp) {
                    console.log("upload done");
                    location.reload(true);
                }
            });
        }
    }

    $("#uploaderlabel").on('change', '#filetoupload', function () {
        console.log("uploading image");
        uploadFile();
    });

    $("#csp").on('change', function () {
        $("#csp_status").show();
        var s = $("#csp").val() == "true" ? true : false;

        $.ajax({
            url: "/sett" + "ings",
            type: 'POST',
            cache: false,
            data: { csp: s },
            success: function (resp) {
                console.log("csp sent " + resp);
                if (resp.toLowerCase() === "true" || resp.toLowerCase() === "false") {
                    $("#csp_status").hide();
                }
            },
            error: function (resp) {
                $("#csp_status").hide();
            }
        });

    });


    // When loaded, add the images to a carousel
    $.get("/images", function (resp) {
        if (resp.length > 0) {
            $(".carousel-inner").append(resp);
            $("#myCarousel").carousel({ interval: 10000 });
        }
    });

});

