// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
   
    $("body").on("click", "#addDataModel", function () { // For Add Data Form 
        console.log(" Add Button clicked");
        $.get("/DID/Add", function (data) {
            $(".modal-body").html(data);
        })
    })


    $("body").on("click", ".generateRange", function (e) { // for Generate Range 
        console.log("renage Generaion Button Cliked");
        let start = parseInt($("#startRange").val());
        let end = parseInt($("#endRange").val());

        if (isNaN(start) || isNaN(end) || start > end) {
            $.toast({
                heading: 'Error',
                text: 'Invalid Range Inserted !',
                showHideTransition: 'fade',
                icon: 'error'
            })
            return;
        }
        console.log("starting Range", start);
        console.log("ending Range", end);

        let numbers = [];
        for (let i = start; i <= end; i++) {
            numbers.push(i);
        }

        $("#w3review").val(numbers.join("\n")).trigger("input");
    });

    $("body").on("input change", "#w3review", function () { // For Getting the Range List 
        console.log("Textarea changed: ", $(this).val());
        let lines = $(this).val().split("\n").filter(l => l.trim() !== "");

        let rows = "";
        lines.forEach((val, i) => {
            rows += `
            <tr data-did="${val}">
                <td>
                    <input type="hidden" name="[${i}].DID" value="${val}" />
                    ${val}
                    <div class="text-danger small error-cell"></div> <!-- inline error -->
                </td>
                <td>
                    <input type="text" name="[${i}].City" class="form-control" value="All" placeholder="Enter city" />
                </td>
                <td>
                    <input type="text" name="[${i}].Country" class="form-control" value="All" placeholder="Enter country" />
                </td>
                <td>
                    <input type="hidden" name="[${i}].NumberType" value="GEOGRAPHICAL" />
                    Default
                </td>
            </tr>
            `;
        });

        $("#AddGeneratedDataTable").html(rows);
    });

    
    $("body").on("click", ".save-btn", function (e) { //For Save the Added Data 
        e.preventDefault();
        const formData = $("#addGeneratedForm").serialize();

        $.post("/DID/Add", formData, function (response) {
            // Clear old errors
            $(".error-cell").text("");

            if (response.success) {
                $("#myModal").modal("hide");
                getData();
                $.toast({
                    heading: 'Success',
                    text: 'DID(s) added successfully',
                    showHideTransition: 'slide',
                    icon: 'success'
                });
            }

            // Show row-wise errors
            $.each(response.errors, function (did, message) {
                $(`#AddGeneratedDataTable tr[data-did="${did}"] .error-cell`).text(message);
            });
        });
    });

    $(document).on("click", ".deleteBtn", function () { // For Delete the specific DID
        const did = $(this).data("id");

        Swal.fire({
            title: 'Are you sure?',
            text: "This will Never RollBack",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, delete it!',
            cancelButtonText: 'No, cancel',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {

                $.post("/DID/Delete", { did: did }, function (response) {
                    if (response.success) {
                        Swal.fire('Deleted!', 'The DID has been removed.', 'success');
                        getData();
                    } else {
                        Swal.fire('Error!', 'Something went wrong.', 'error');
                    }
                });
            }
        });
    });

    $(document).on("click", ".updateBtn", function () { // For Update the specific DID
        const did = $(this).data("id");

        $.get("/DID/update", { did: did }, function (response) {
                    $("#myModalLabel").text("Update");
                    $(".modal-body").html(response);
                    $("#myModal").modal("show");
                });
    });

    $(document).on("submit", "#updateDIDData", function (e) {
        console.log("submit event triggered");
        e.preventDefault();
        const formData = $(this).serialize();

        $.post("/DID/Update", formData, function (response) {
           
            if (response.success) {
                $("#myModal").modal("hide");
                getData();
                $.toast({
                    heading: 'Success',
                    text: 'Data Updated Successfully',
                    showHideTransition: 'slide',
                    icon: 'success'
                });
            }

        });
    })
});
