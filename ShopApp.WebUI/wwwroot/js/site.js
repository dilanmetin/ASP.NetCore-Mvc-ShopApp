// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $("#name").keyup(function () {
        var name = $("#name").val();
        $(".card-name").text(name)
    });
    $("#card-number").keyup(function () {
        var nmbr = $("#card-number").val();
        var nmbr1 = nmbr.slice(0, 4) + " ";
        var nmbr2 = nmbr.slice(4, 8) + " ";
        var nmbr3 = nmbr.slice(8, 12) + " ";
        var nmbr4 = nmbr.slice(12, 16);
        var card = nmbr1 + nmbr2 + nmbr3 + nmbr4;
        
        $(".number").text(card)
    });
    $("#exp-month").keyup(function () {
        var month = $("#exp-month").val();
        $(".month").text(month)
    });
    $("#exp-year").keyup(function () {
        var year = $("#exp-year").val();
        $(".year").text(year)
    });
    $("#cvv").keyup(function () {
        var cvv = $("#cvv").val();
        $(".cvv").text(cvv)
    });
});