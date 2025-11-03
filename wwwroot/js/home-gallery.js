// Home Gallery - 前台相片瀏覽功能
$(document).ready(function() {
    // ---- 主題：載入與切換 ----
    const THEME_KEY = 'theme';
    
    function setIconFor(theme) {
        const $icon = $('#themeToggleIcon');
        if (theme === 'light') {
            // 太陽圖示
            $icon.html('<circle cx="12" cy="12" r="5"></circle>\
                        <line x1="12" y1="1" x2="12" y2="3"></line>\
                        <line x1="12" y1="21" x2="12" y2="23"></line>\
                        <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>\
                        <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>\
                        <line x1="1" y1="12" x2="3" y2="12"></line>\
                        <line x1="21" y1="12" x2="23" y2="12"></line>\
                        <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>\
                        <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>');
        } else {
            // 月亮圖示
            $icon.html('<path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>');
        }
    }

    function applyTheme(theme) {
        if (theme === 'light') {
            document.body.classList.add('light');
        } else {
            document.body.classList.remove('light');
            theme = 'dark';
        }
        localStorage.setItem(THEME_KEY, theme);
        setIconFor(theme);
    }

    // 初始化主題
    const savedTheme = localStorage.getItem(THEME_KEY);
    if (savedTheme) {
        applyTheme(savedTheme);
    } else {
        // 若無偏好，依系統設定
        const prefersLight = window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches;
        applyTheme(prefersLight ? 'light' : 'dark');
    }

    // 主題切換按鈕
    $('#themeToggle').on('click', function () {
        const current = localStorage.getItem(THEME_KEY) || 'dark';
        applyTheme(current === 'dark' ? 'light' : 'dark');
    });

    // 從 window 物件取得資料（由 View 注入）
    const photos = window.galleryPhotos || [];
    const isAuthenticated = window.isAuthenticated || false;
    const isAdmin = window.isAdmin || false;
    const currentUserId = window.currentUserId || '';

    // 初始化編輯按鈕狀態
    if (!isAuthenticated) {
        $('#modalEditBtn').hide();
    }

    let currentFilter = 'all';
    let currentPhotoIndex = 0;
    let filteredPhotos = photos;

    // 取得所有成員
    const members = ['all', ...new Set(photos.map(p => p.member))];

    // Helper to resolve image URLs
    function resolveImageUrl(path) {
        if (!path) {
            return 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="800" height="600"></svg>';
        }
        if (/^(https?:)?\/\//i.test(path) || path.startsWith('/')) return path;
        return '/uploads/' + path;
    }

    // 初始化篩選器
    function initFilters() {
        const filterContainer = $('#filterContainer');
        filterContainer.empty();

        members.forEach(member => {
            const btn = $('<button>')
                .addClass('filter-btn')
                .text(member === 'all' ? '全部' : member)
                .data('member', member)
                .on('click', function() {
                    currentFilter = $(this).data('member');
                    $('.filter-btn').removeClass('active');
                    $(this).addClass('active');
                    renderGallery();
                });

            if (member === 'all') {
                btn.addClass('active');
            }

            filterContainer.append(btn);
        });
    }

    // 渲染相片網格
    function renderGallery() {
        filteredPhotos = currentFilter === 'all'
            ? photos
            : photos.filter(p => p.member === currentFilter);

        const gallery = $('#galleryGrid');
        gallery.empty();

        if (filteredPhotos.length === 0) {
            $('#emptyState').show();
            return;
        }

        $('#emptyState').hide();

        filteredPhotos.forEach((photo, index) => {
            const imgUrl = resolveImageUrl(photo.imageUrl);

            let cardHtml = `
                <div class="photo-card" data-index="${index}">
                    <img src="${imgUrl}" alt="${photo.title}">
                    <div class="photo-overlay"></div>
                    <div class="photo-info">
                        <div class="photo-tags">
                            <span class="photo-member">${photo.member}</span>
                            <span class="photo-date">${photo.date}</span>
                        </div>
                        <h3 class="photo-title">${photo.title}</h3>
                        <p class="photo-description">${photo.description}</p>
                    </div>
                    ${(isAuthenticated && (photo.userId === currentUserId || isAdmin)) ? `<button class="edit-btn" data-id="${photo.id}" title="編輯回憶"><svg class="icon-sm" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"></path></svg></button>` : ''}
                </div>
            `;

            const card = $(cardHtml);

            card.on('click', function(e) {
                if ($(e.target).closest('.edit-btn').length) {
                    openEditModal($(e.target).closest('.edit-btn').data('id'));
                } else {
                    openModal($(this).data('index'));
                }
            });

            gallery.append(card);
        });
    }

    // 開啟 Modal
    function openModal(index) {
        currentPhotoIndex = index;
        updateModal();
        $('#photoModal').addClass('active');
        $('body').css('overflow', 'hidden');
    }

    // 關閉 Modal
    function closeModal() {
        $('#photoModal').removeClass('active');
        $('body').css('overflow', 'auto');
    }

    // 更新 Modal 內容
    function updateModal() {
        const photo = filteredPhotos[currentPhotoIndex];
        const imgUrl = resolveImageUrl(photo.imageUrl);
        $('#modalImage').attr('src', imgUrl).attr('alt', photo.title);
        $('#modalMember').text(photo.member);
        $('#modalDate').text(photo.date);
        $('#modalTitle').text(photo.title);
        $('#modalDescription').text(photo.description);
        $('#modalCounter').text(`#${currentPhotoIndex + 1} / ${filteredPhotos.length}`);

        // 控制「編輯」按鈕顯示 - 必須登入且（是自己的照片或是管理員）
        const canEdit = isAuthenticated && (photo.userId === currentUserId || isAdmin);
        if (canEdit) {
            $('#modalEditBtn').show();
        } else {
            $('#modalEditBtn').hide();
        }
    }

    // 上一張
    function prevPhoto() {
        currentPhotoIndex = (currentPhotoIndex - 1 + filteredPhotos.length) % filteredPhotos.length;
        updateModal();
    }

    // 下一張
    function nextPhoto() {
        currentPhotoIndex = (currentPhotoIndex + 1) % filteredPhotos.length;
        updateModal();
    }

    // 事件綁定
    $('#modalClose').on('click', closeModal);
    $('#modalPrev').on('click', prevPhoto);
    $('#modalNext').on('click', nextPhoto);

    // 鍵盤事件
    $(document).on('keydown', function(e) {
        if ($('#photoModal').hasClass('active')) {
            if (e.key === 'Escape') closeModal();
            if (e.key === 'ArrowLeft') prevPhoto();
            if (e.key === 'ArrowRight') nextPhoto();
        } else if ($('#editModal').hasClass('active')) {
            if (e.key === 'Escape') closeEditModal();
        } else if ($('#addModal').hasClass('active')) {
            if (e.key === 'Escape') closeAddModal();
        }
    });

    // 點擊背景關閉
    $('#photoModal').on('click', function(e) {
        if (e.target === this) {
            closeModal();
        }
    });

    // Add Memory Modal
    $('#addMemoryBtn').on('click', function() {
        $('#addModal').addClass('active');
        $('body').css('overflow', 'hidden');
    });

    $('#addModalClose').on('click', closeAddModal);

    function closeAddModal() {
        $('#addModal').removeClass('active');
        $('body').css('overflow', 'auto');
    }

    $('#addModal').on('click', function(e) {
        if (e.target === this) {
            closeAddModal();
        }
    });

    $('#addForm').on('submit', function(e) {
        e.preventDefault();
        const formData = new FormData();
        formData.append('title', $('#addTitle').val());
        formData.append('description', $('#addDescription').val());
        formData.append('date', $('#addDate').val());
        formData.append('file', $('#addImage')[0].files[0]);

        $.ajax({
            url: '/api/memories',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(newMemory) {
                location.reload();
            },
            error: function(xhr) {
                alert('新增失敗: ' + (xhr.responseText || '請稍後再試'));
            }
        });
    });

    // Edit Modal
    function openEditModal(photoId) {
        // 權限檢查：未登入時不允許開啟編輯 modal
        if (!isAuthenticated) {
            return;
        }

        const photo = photos.find(p => p.id === photoId);
        if (photo) {
            // 再次檢查權限：必須是自己的照片或是管理員
            const canEdit = (photo.userId === currentUserId || isAdmin);
            if (!canEdit) {
                return;
            }

            $('#editPhotoId').val(photo.id);
            $('#editTitle').val(photo.title);
            $('#editDescription').val(photo.description);
            $('#editCurrentImage').attr('src', resolveImageUrl(photo.imageUrl));
            $('#editImage').val('');
            $('#editModal').addClass('active');
            $('body').css('overflow', 'hidden');
        }
    }

    function closeEditModal() {
        $('#editModal').removeClass('active');
        $('body').css('overflow', 'auto');
    }

    $('#editModalClose').on('click', closeEditModal);

    $('#editModal').on('click', function(e) {
        if (e.target === this) {
            closeEditModal();
        }
    });

    $('#editForm').on('submit', function(e) {
        e.preventDefault();
        const photoId = $('#editPhotoId').val();
        const formData = new FormData();
        formData.append('title', $('#editTitle').val());
        formData.append('description', $('#editDescription').val());
        
        const imageFile = $('#editImage')[0].files[0];
        if (imageFile) {
            formData.append('file', imageFile);
        }

        $.ajax({
            url: `/api/memories/${photoId}`,
            type: 'PUT',
            data: formData,
            processData: false,
            contentType: false,
            success: function(updatedPhoto) {
                const photoIndex = photos.findIndex(p => p.id == photoId);
                if (photoIndex !== -1) {
                    photos[photoIndex].title = updatedPhoto.title;
                    photos[photoIndex].description = updatedPhoto.description;
                    if (updatedPhoto.imageUrl) {
                        photos[photoIndex].imageUrl = updatedPhoto.imageUrl;
                    }
                }
                renderGallery();
                closeEditModal();
                if ($('#photoModal').hasClass('active')) {
                    updateModal();
                }
            },
            error: function(xhr) {
                alert('更新失敗: ' + (xhr.responseText || '請稍後再試'));
            }
        });
    });

    // Modal 中的「編輯」按鈕行為
    $('#modalEditBtn').on('click', function (e) {
        e.preventDefault();
        const photo = filteredPhotos[currentPhotoIndex];
        if (!photo) return;
        if (isAuthenticated && (photo.userId === currentUserId || isAdmin)) {
            openEditModal(photo.id);
        }
    });

    // 初始化
    initFilters();
    renderGallery();
});
