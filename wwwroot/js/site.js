$(document).ready(function () {
    // ---- Dynamic Grid Logic ---- 
    function handleDynamicRow($tableBody) {
        $tableBody.off('input', 'tr:last-child input');
        $tableBody.on('input', 'tr:last-child input', function () {
            var val = $(this).val();
            if (val.length > 0) {
                var $currentRow = $(this).closest('tr');
                var $newRow = $currentRow.clone();
                $newRow.find('input').val('');
                $tableBody.append($newRow);
                handleDynamicRow($tableBody);
            }
        });
    }

    handleDynamicRow($('#paramsTable tbody'));
    handleDynamicRow($('#headersTable tbody'));
    handleDynamicRow($('#formDataTable tbody'));
    handleDynamicRow($('#envTable tbody'));

    $(document).on('click', '.delete-row', function () {
        var $row = $(this).closest('tr');
        var $tbody = $row.closest('tbody');
        if ($tbody.find('tr').length > 1 && !$row.is(':last-child')) {
            $row.remove();
        } else if ($row.is(':last-child')) {
            $row.find('input').val('');
        }
    });

    // Method Badges sync
    $('.method-select').change(function () {
        var val = $(this).val();
        $(this).removeClass('text-danger text-warning text-primary text-success');
        if (val === 'GET') {
            $(this).css('color', '#819fff');
            $('.url-input').css('color', '#819fff');
        } else if (val === 'POST') {
            $(this).addClass('text-warning');
            $('.url-input').addClass('text-warning');
        } else if (val === 'PUT') {
            $(this).addClass('text-primary');
            $('.url-input').css('color', '#50e3c2');
        } else if (val === 'DELETE') {
            $(this).addClass('text-danger');
            $('.url-input').addClass('text-danger');
        }
    });
    $('.method-select').trigger('change');

    // ---- UI Switching Logic ----
    // Body Types
    $('input[name="bodyType"]').change(function () {
        var val = $(this).val();
        if (val === 'none') {
            $('#rawBodyEditor').addClass('d-none');
            $('#formDataEditor').addClass('d-none');
        } else if (val === 'raw') {
            $('#rawBodyEditor').removeClass('d-none');
            $('#formDataEditor').addClass('d-none');
        } else if (val === 'formdata' || val === 'urlencoded') {
            $('#rawBodyEditor').addClass('d-none');
            $('#formDataEditor').removeClass('d-none');
        }
    });

    // Auth Types
    $('#authTypeSelect').change(function () {
        var val = $(this).val();
        $('#authBearerSettings, #authBasicSettings, #authNoneSettings').addClass('d-none');
        if (val === 'bearer') {
            $('#authBearerSettings').removeClass('d-none');
        } else if (val === 'basic') {
            $('#authBasicSettings').removeClass('d-none');
        } else {
            $('#authNoneSettings').removeClass('d-none');
        }
    });

    // ---- Environment Variable Interpolation ----
    function getEnvVariables() {
        var vars = {};
        $('#envTable tbody tr').each(function () {
            var key = $(this).find('.env-key').val();
            var val = $(this).find('.env-val').val();
            if (key) vars[key] = val;
        });
        return vars;
    }

    function interpolateString(str, envVars) {
        if (!str) return str;
        var result = str;
        for (var key in envVars) {
            var regex = new RegExp('{{' + key + '}}', 'g');
            result = result.replace(regex, envVars[key]);
        }
        return result;
    }

    // ---- API Execution Logic ---- 
    $('#btn-send').click(function () {
        var envVars = getEnvVariables();
        var rawUrl = $('.url-input').val();
        var url = interpolateString(rawUrl, envVars);
        var method = $('.method-select').val();

        // Collect Headers
        var headers = {};
        $('#headersTable tbody tr').each(function () {
            var key = interpolateString($(this).find('.header-key').val(), envVars);
            var val = interpolateString($(this).find('.header-val').val(), envVars);
            if (key) headers[key] = val;
        });

        // Collect Auth
        var authType = $('#authTypeSelect').val();
        if (authType === 'bearer') {
            var token = interpolateString($('#authBearerToken').val(), envVars);
            if (token) headers['Authorization'] = 'Bearer ' + token;
        } else if (authType === 'basic') {
            var user = interpolateString($('#authBasicUser').val(), envVars);
            var pass = interpolateString($('#authBasicPass').val(), envVars);
            if (user || pass) {
                headers['Authorization'] = 'Basic ' + btoa(user + ':' + pass);
            }
        }

        // Collect Params
        var queryParams = [];
        $('#paramsTable tbody tr').each(function () {
            var key = interpolateString($(this).find('.param-key').val(), envVars);
            var val = interpolateString($(this).find('.param-val').val(), envVars);
            if (key) {
                queryParams.push(encodeURIComponent(key) + '=' + encodeURIComponent(val));
            }
        });

        var finalUrl = url;
        if (queryParams.length > 0) {
            finalUrl += (url.indexOf('?') === -1 ? '?' : '&') + queryParams.join('&');
        }

        // Collect Body
        var bodyType = $('input[name="bodyType"]:checked').val();
        var bodyContent = "";
        var requestContentType = headers['Content-Type']; // Respect user override if present

        if (bodyType === 'raw') {
            bodyContent = interpolateString($('#requestBody').val(), envVars);
            if (!requestContentType) requestContentType = 'application/json';
        } else if (bodyType === 'formdata' || bodyType === 'urlencoded') {
            var formDataObj = {};
            $('#formDataTable tbody tr').each(function () {
                var key = interpolateString($(this).find('.form-key').val(), envVars);
                var val = interpolateString($(this).find('.form-val').val(), envVars);
                if (key) formDataObj[key] = val;
            });
            bodyContent = JSON.stringify(formDataObj); // We'll send this Map to the C# proxy to construct the final payload
            if (!requestContentType) requestContentType = bodyType === 'formdata' ? 'multipart/form-data' : 'application/x-www-form-urlencoded';
        }

        $('#responseOutput').text("Sending...");
        $('#respStatus').html('Status: <span class="text-warning fw-bold">Pending</span>');
        $('#respTime').html('');
        var startTime = performance.now();

        var requestData = {
            method: method,
            url: finalUrl,
            headersJson: JSON.stringify(headers),
            bodyType: bodyType,
            contentType: requestContentType,
            body: bodyContent
        };

        $.ajax({
            url: '/ApiProxy/Execute',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response, status, xhr) {
                var endTime = performance.now();
                setResponseMetrics(xhr.status, xhr.statusText, Math.round(endTime - startTime), true);
                try {
                    var parsed = JSON.parse(response);
                    $('#responseOutput').text(JSON.stringify(parsed, null, 4));
                } catch (e) {
                    $('#responseOutput').text(response);
                }
            },
            error: function (xhr, status, error) {
                var endTime = performance.now();
                setResponseMetrics(xhr.status, error, Math.round(endTime - startTime), false);
                if (xhr.responseText) {
                    try {
                        var parsed = JSON.parse(xhr.responseText);
                        $('#responseOutput').text(JSON.stringify(parsed, null, 4));
                    } catch (e) {
                        $('#responseOutput').text(xhr.responseText);
                    }
                } else {
                    $('#responseOutput').text("Network Error or blocked by CORS.");
                }
            }
        });
    });

    function setResponseMetrics(statusCode, statusText, timeMs, isSuccess) {
        var colorClass = isSuccess || (statusCode >= 200 && statusCode < 300) ? 'text-success' : 'text-danger';
        $('#respStatus').html(`Status: <span class="${colorClass} fw-bold">${statusCode} ${statusText}</span>`);
        $('#respTime').html(`Time: <span class="text-success fw-bold">${timeMs} ms</span>`);

        if (isSuccess) {
            $('#responseOutput').removeClass('text-danger').addClass('text-success');
        } else {
            $('#responseOutput').addClass('text-danger').removeClass('text-success');
        }
    }
});
