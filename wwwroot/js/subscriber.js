
$(document).ready(function () {

    $(document).on("click", ".addSubModel", function () { // For getting the add form 
        //console.log(" Add Button clicked");
        $.get("/Subscriber/Add", function (data) {
            $("#myModalLabel").text("Add Subscriber");
            $(".modal-body").html(data);
            $("#myModal").modal("show");
        })
    })

    $(document).on("submit", "#addSubscriberData", function (e) {
        e.preventDefault();

        const formDataArray = $(this).serializeArray();

        const formData = {};

        $.map(formDataArray, function (n, i) {
            formData[n['name']] = n['value'];
        });

        console.log(formData);

        $.ajax({
            url: "/Subscriber/Add",          
            type: "POST",
            contentType: "application/json", 
            data: JSON.stringify(formData),
            success: function (response) {
                $(".error-cell").text("");
                if (response.success) {
                    console.log("Saved Successfully:", response);
                    $("#myModal").modal("hide");
                    getSubData();
                    $.toast({
                        heading: 'Success',
                        text: 'Subscriber added successfully!',
                        showHideTransition: 'slide',
                        icon: 'success'
                    })
                }
                else {
                    $.each(response.message, function (key, val) {
                        $("#" + key + "Error").text(val);
                    });
                    $.toast({
                        heading: 'Error',
                        text: 'Please check the information again',
                        showHideTransition: 'fade',
                        icon: 'error'
                    })
                }
                
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                alert("Something went wrong!");
            }
        });
    })

    $(document).on("click", ".updateSubBtn", function () {
        //alert("update button clicked");
        const id = $(this).data("id");

        $.get("/Subscriber/update", { id: id }, function (response) {

            $("#myModalLabel").text("Update");
            $(".modal-body").html(response);
            $("#myModal").modal("show");
        });
    })

    $(document).on("click", ".deleteSubBtn", function () { // For Delete the 
        const id = $(this).data("id");
        console.log(id)
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

                $.post("/Subscriber/Delete", { id: id }, function (response) {
                    if (response.success) {
                        Swal.fire('Deleted!', 'The DID has been removed.', 'success');
                        getSubData();
                    } else {
                        Swal.fire('Error!', 'Something went wrong.', 'error');
                    }
                });
            }
        });
    });

    $(document).on("submit", "#updateSubscriberData", function (e) {
        e.preventDefault();

        const formDataArray = $(this).serializeArray();

        const formData = {};

        $.map(formDataArray, function (n, i) {
            formData[n['name']] = n['value'];
        });

        $.ajax({
            url: "/Subscriber/Update",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                $(".error-cell").text("");
                if (response.success) {
                    console.log("Saved Successfully:", response);
                    $("#myModal").modal("hide");
                    getSubData();
                    $.toast({
                        heading: 'Success',
                        text: 'Subscriber Updated successfully!',
                        showHideTransition: 'slide',
                        icon: 'success'
                    })
                }
                else {
                    $.each(response.message, function (key, val) {
                        $("#" + key + "Error").text(val);
                    });
                    $.toast({
                        heading: 'Error',
                        text: 'Please check the information again',
                        showHideTransition: 'fade',
                        icon: 'error'
                    })
                    getSubData();
                }

            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                alert("Something went wrong!");
            }
        });
    })

    $(document).on("click", ".searchSubBtn", function () {
        console.log("search button clicked");
        var filter = {
            customer_name: $("#searchCustomer").val().trim(),
            username: $("#searchUsername").val().trim(),
        }
        if (filter.username || filter.customer_name) {
            $.ajax({
                url: "/Subscriber/GetData",
                method: "GET",
                data: filter,
                success: function (response) {
                    $(".subscribertable").html(response);

                },
                error: function (xhr, status, error) {
                    console.error("Error:", error);
                }
            });

        } else {
            $.toast({
                heading: 'Warning',
                text: 'Please insert Some Values First',
                showHideTransition: 'plain',
                icon: 'warning'
            })

        }
    })

    $(document).on('click', '.resetSubBtn', function () {
        console.log("reset clicked");
        $("#searchCustomer").val("");
        $("#searchUsername").val("");
        loadSubscriberList({ pageNumber: 0, pageSize: pageSizelength })

    })
})