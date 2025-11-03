// Admin - 使用者管理功能
$(document).ready(function() {
    // 搜尋功能
    $('#searchInput').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        $('#usersTable tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // 狀態切換
    $('.toggle-status-btn').on('click', function() {
        const form = $(this).closest('form');
        const username = $(this).data('username');
        const isActive = $(this).data('active');
        const action = isActive ? '停用' : '啟用';
        
        if (confirm(`確定要${action}使用者「${username}」嗎？`)) {
            form.submit();
        }
    });

    // 刪除確認
    $('.delete-btn').on('click', function(e) {
        const username = $(this).data('username');
        if (!confirm(`確定要刪除使用者「${username}」嗎？此操作無法復原。`)) {
            e.preventDefault();
        }
    });
});
