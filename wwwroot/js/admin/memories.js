// Admin - 回憶管理功能
$(document).ready(function() {
    // 搜尋功能
    $('#searchInput').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        $('#memoriesTable tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // 刪除按鈕
    $('.delete-btn').on('click', function() {
        const memoryId = $(this).data('id');
        const row = $(this).closest('tr');
        const title = row.find('strong').text();
        
        if (confirm(`確定要刪除「${title}」嗎？此操作無法復原。`)) {
            $('#deleteMemoryId').val(memoryId);
            $('#deleteForm').submit();
        }
    });

    // 表格排序（簡單版）
    $('th').on('click', function() {
        const table = $(this).parents('table').eq(0);
        const rows = table.find('tbody tr').toArray().sort(comparer($(this).index()));
        this.asc = !this.asc;
        if (!this.asc) {
            rows = rows.reverse();
        }
        for (let i = 0; i < rows.length; i++) {
            table.append(rows[i]);
        }
    });

    function comparer(index) {
        return function(a, b) {
            const valA = getCellValue(a, index);
            const valB = getCellValue(b, index);
            return $.isNumeric(valA) && $.isNumeric(valB) ? valA - valB : valA.localeCompare(valB);
        };
    }

    function getCellValue(row, index) {
        return $(row).children('td').eq(index).text();
    }
});
