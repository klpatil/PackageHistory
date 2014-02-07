$(document).ready(function () {
    $('#rootwizard').bootstrapWizard({
        onTabClick: function (tab, navigation, index) {
            return false;
        }
    });
});

var frameSrc = "UninstallPackage.aspx?PKGID=";
function ShowUninstallDialog(objID) {
    $('#myModal').on('show', function () {
        
        var urlToOpen = frameSrc + objID;
        
        $('iframe').attr("src", urlToOpen);
    });

    $('#myModal').modal({ show: true })

}
var confirmed = false;

function ShowConfirm(controlID) {
    if (confirmed) { return true; }

    bootbox.confirm("Are you sure want to continue?", function (result) {
        if (result) {
            if (controlID != null) {
                var controlToClick = document.getElementById(controlID);
                if (controlToClick != null) {
                    confirmed = true;
                    controlToClick.click();
                    confirmed = false;
                }
            }
        }

    });

    return false;

}


