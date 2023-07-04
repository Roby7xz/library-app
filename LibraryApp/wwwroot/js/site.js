// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showModal(id, url) {
    $.ajax({
        type: "GET",
        url: url,
        data: {
            Id: id
        },
        success: function (data) {
            $("body").append(data);
            disableBody();
            $(".modal_overlay").addClass("modal_show_animation");
        }
    })
}

function showConfirmationModal(message, onConfirmCallback) {
    $("body").append(`
    <div class="modal_overlay">
        <div class="modal_container">
            <div class="modal_header">
                ${message}
            </div>
            <div class="modal_body">
            <button id="cancel" class="btn btn-primary">Cancel</button>
            <button id="confirm" class="btn btn-danger">Delete</button>
            </div>           
        </div>
    </div>
    `);
    disableBody();
    $(".modal_overlay").addClass("modal_show_animation");
    
    $("#cancel").click(function () {
        hideModal();
        enableBody();
    });

    $("#confirm").click(function () {    
        onConfirmCallback();
        hideModal();
        enableBody();
    });  
}

function hideModal() {
    $(".modal_overlay").addClass("modal_hide_animation");
    enableBody();
    setTimeout(function () {
        $(".modal_overlay").remove();     
    }, 800);  
}

function showLoader() {
    $("#loader").show();
}

function hideLoader() {
    $("#loader").hide();
}

function disableBody() {
    $("body").addClass("body-disabled");
}

function enableBody() {
    $("body").removeClass("body-disabled");
}

function showZoomedInImage(event) {
    var src = $(event.target).attr('src');
    disableBody();

    $('<div>').addClass('zoomed_in_image').prepend($('<img>', { src: src })).click(function () {
        $(this).remove();
        enableBody();
    }).appendTo("body");
}

function showPreviewImage(event) {
    var file = event.target.files[0];

    if (!file) {
        $('#image_preview').attr('src', "");
    }

    var reader = new FileReader();

    reader.onload = function (e) {
        $('#image_preview').attr('src', e.target.result);
    }

    reader.readAsDataURL(file);
}


function initializeDownload(data, xhr) {

}