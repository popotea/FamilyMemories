// Admin - 角色管理功能
$(document).ready(function() {
    // 搜尋功能
    $('#searchInput').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        $('#rolesTable tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // 刪除確認
    $('.delete-role-btn').on('click', function(e) {
        const roleName = $(this).data('role');
        if (!confirm(`確定要刪除角色「${roleName}」嗎？此操作無法復原。`)) {
            e.preventDefault();
        }
    });

    // 權限選擇
    $('#selectAllPermissions').on('change', function() {
        $('.permission-checkbox').prop('checked', $(this).is(':checked'));
    });
});
