const API_URL = "https://storehub-alper.azurewebsites.net/api";

const app = {
    state: {
        view: 'home',
        cart: [],
        token: localStorage.getItem('token') || null,
        user: null,
        productParams: {
            pageNumber: 1,
            pageSize: 8,
            search: '',
            minPrice: '',
            maxPrice: '',
            category: ''
        },
        adminProductParams: {
            category: ''
        },
        categories: []
    },

    async init() {
        if(this.state.token) {
            this.decodeUser();
        }
        await this.fetchCategories();
        this.renderNav();
        this.navigate('home');
    },

    async fetchCategories() {
        try {
            const res = await fetch(`${API_URL}/Products/categories`);
            if (res.ok) {
                this.state.categories = await res.json();
                // Kategoriler geldiğinde mevcut görünümdeki dropdown'ları güncelle
                this.populateCategoriesDropdown('home-category');
                this.populateCategoriesDropdown('admin-category');
            }
        } catch (e) {
            console.error("Kategoriler çekilemedi:", e);
        }
    },

    decodeUser() {
        try {
            const payload = JSON.parse(atob(this.state.token.split('.')[1]));
            // System'de jwt claim'lerine göre role:
            const roleKey = Object.keys(payload).find(k => k.includes('role'));
            const usernameKey = Object.keys(payload).find(k => k.includes('name'));
            
            this.state.user = {
                username: payload[usernameKey] || 'Kullanıcı',
                role: payload[roleKey] || 'Customer'
            };
        } catch(e) {
            this.logout();
        }
    },

    renderNav() {
        const authContainer = document.getElementById('auth-buttons');
        const ordersNav = document.getElementById('nav-orders');
        const adminProductsNav = document.getElementById('nav-admin-products');

        if(this.state.token && this.state.user) {
            authContainer.innerHTML = `<button class="btn-secondary" onclick="app.logout()"><i class="fa-solid fa-right-from-bracket"></i> Çıkış Yap</button>`;
            if(this.state.user.role === 'Admin') {
                ordersNav.classList.remove('hidden');
                adminProductsNav.classList.remove('hidden');
            } else {
                ordersNav.classList.add('hidden');
                adminProductsNav.classList.add('hidden');
            }
        } else {
            authContainer.innerHTML = `<button class="btn-primary" onclick="app.navigate('auth')"><i class="fa-solid fa-user"></i> Giriş / Kayıt</button>`;
            ordersNav.classList.add('hidden');
            adminProductsNav.classList.add('hidden');
        }
    },

    navigate(view) {
        this.state.view = view;
        const main = document.getElementById('app-content');
        const template = document.getElementById(`view-${view}`);
        
        main.innerHTML = '';
        main.appendChild(template.content.cloneNode(true));

        if(view === 'home') {
            if(this.state.user) {
                document.getElementById('welcome-name').textContent = this.state.user.username;
            }
            this.loadProducts();
        } else if(view === 'orders') {
            this.loadOrders();
        } else if(view === 'admin-products') {
            this.populateCategoriesDropdown('admin-category');
            this.loadAdminProducts();
        }
    },

    populateCategoriesDropdown(selectId) {
        const select = document.getElementById(selectId);
        if (!select) return;
        select.innerHTML = '<option value="">Tüm Kategoriler</option>';
        this.state.categories.forEach(cat => {
            select.innerHTML += `<option value="${cat}">${cat}</option>`;
        });
    },

    // ================== URUNLER & CACHING ==================
    async loadProducts() {
        const monitor = document.getElementById('api-speed-monitor');
        monitor.className = 'speed-monitor speed-db';
        monitor.innerText = 'Yükleniyor... (PostgreSQL / Cache Sorgusu)';

        const { pageNumber, pageSize, search, minPrice, maxPrice, category } = this.state.productParams;
        const query = new URLSearchParams({ PageNumber: pageNumber, PageSize: pageSize });
        if(search) query.append('Search', search);
        if(minPrice) query.append('MinPrice', minPrice);
        if(maxPrice) query.append('MaxPrice', maxPrice);
        if(category) query.append('Category', category);

        this.populateCategoriesDropdown('home-category');
        document.getElementById('home-category').value = category;

        try {
            const start = performance.now();
            const res = await fetch(`${API_URL}/Products?${query}`);
            const data = await res.json();
            const end = performance.now();
            const speed = (end - start).toFixed(2);

            if(speed < 15) {
                monitor.className = 'speed-monitor speed-cache';
                monitor.innerHTML = `⚡ IMemoryCache Kullanıldı! Hız: ${speed}ms (DB Yorulmadı)`;
            } else {
                monitor.className = 'speed-monitor speed-db';
                monitor.innerHTML = `🐌 Veritabanından Çekildi. Hız: ${speed}ms (Sonraki F5'lerde hızı gör!)`;
            }

            const grid = document.getElementById('product-list');
            grid.innerHTML = '';

            // PagedResponse Formatından (items propertysi) alıyoruz
            const itemsList = data.items || data; 
            
            if(!itemsList || itemsList.length === 0) {
                grid.innerHTML = `<p style="grid-column: 1/-1; text-align: center;">Ürün bulunamadı.</p>`;
            } else {
                itemsList.forEach(p => {
                // EĞER resim adresi 'http' ile başlıyorsa (Dışarıdan geliyorsa: DummyJSON) DİREKT KULLAN!
                // Başlamıyorsa (İçeriden yüklenmişse) bizim Sunucu adresimizi (8080) başına ekle.
                const imgSource = p.imageUrl 
                    ? (p.imageUrl.startsWith('http') ? p.imageUrl : `https://storehub-alper.azurewebsites.net${p.imageUrl}`) 
                    : null;
                const imgHtml = imgSource 
                    ? `<img src="${imgSource}" style="width:100%; height:180px; object-fit:cover; border-radius:8px; margin-bottom:1rem;">`
                    : `<div class="img-placeholder"><i class="fa-solid fa-image fa-3x"></i></div>`;

                grid.innerHTML += `
                    <div class="product-card glass-panel">
                        ${imgHtml}
                        <h3>${p.name}</h3>
                        <p>${p.description}</p>
                        <div class="product-price">${p.price} TL</div>
                        <button class="btn-primary add-btn" onclick="app.addToCart(${p.id}, '${p.name}', ${p.price})">
                            <i class="fa-solid fa-cart-plus"></i> Sepete Ekle
                        </button>
                    </div>
                `;
            });
            }

            // Pagination update
            if (data.pageNumber) {
                document.getElementById('page-info').innerText = `Sayfa ${data.pageNumber} / ${data.totalPages}`;
                document.getElementById('btn-prev').disabled = data.pageNumber <= 1;
                document.getElementById('btn-next').disabled = data.pageNumber >= data.totalPages;
            }

        } catch (e) {
            console.error("Products error:", e);
            this.showToast(`Hata Oluştu: ${e.message || 'Sunucuya erişilemiyor'}`, 'error');
        }
    },

    changePage(step) {
        this.state.productParams.pageNumber += step;
        this.loadProducts();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    applyFilters() {
        this.state.productParams.minPrice = document.getElementById('min-price').value;
        this.state.productParams.maxPrice = document.getElementById('max-price').value;
        this.state.productParams.category = document.getElementById('home-category').value;
        this.state.productParams.pageNumber = 1;
        this.loadProducts();
    },

    handleCategoryChange(e) {
        this.state.productParams.category = e.target.value;
        this.state.productParams.pageNumber = 1;
        this.loadProducts();
    },

    handleSearch(e) {
        this.state.productParams.search = e.target.value;
        this.state.productParams.pageNumber = 1;
        this.loadProducts();
    },

    // ================== SEPET (MOCK CART) ==================
    toggleCart() {
        document.getElementById('cart-sidebar').classList.toggle('open');
        this.renderCart();
    },

    addToCart(id, name, price) {
        const item = this.state.cart.find(i => i.productId === id);
        if(item) {
            item.quantity++;
        } else {
            this.state.cart.push({ productId: id, name, price, quantity: 1 });
        }
        this.renderCart();
        this.showToast(`${name} sepete eklendi!`, 'success');
        
        const count = this.state.cart.reduce((a,b) => a + b.quantity, 0);
        document.getElementById('cart-count').innerText = count;
    },

    renderCart() {
        const list = document.getElementById('cart-items');
        list.innerHTML = '';
        let total = 0;

        this.state.cart.forEach(item => {
            const subtotal = item.price * item.quantity;
            total += subtotal;
            list.innerHTML += `
                <div class="cart-item">
                    <div>
                        <strong>${item.name}</strong><br>
                        <small>${item.price} TL x ${item.quantity}</small>
                    </div>
                    <strong>${subtotal.toFixed(2)} TL</strong>
                </div>
            `;
        });

        document.getElementById('cart-total').innerText = total.toFixed(2) + " TL";
    },

    async checkout() {
        if(!this.state.token) {
            this.showToast('Sipariş vermek için Giriş Yapmalısınız!', 'error');
            this.toggleCart();
            this.navigate('auth');
            return;
        }
        if(this.state.cart.length === 0) return this.showToast('Sepetiniz boş!', 'error');

        try {
            const res = await fetch(`${API_URL}/Orders`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.state.token}`
                },
                body: JSON.stringify({
                    items: this.state.cart
                })
            });

            if(!res.ok) {
                const err = await res.json();
                throw new Error(err.message || "Stok yetersiz veya hata oluştu");
            }

            this.showToast('Sipariş Başarıyla Oluşturuldu! (Transaction Mimarisi Kullanıldı)', 'success');
            this.state.cart = [];
            document.getElementById('cart-count').innerText = '0';
            this.toggleCart();

        } catch(e) {
            this.showToast(e.message, 'error');
        }
    },

    // ================== AUTH (JWT) ==================
    authMode: 'login',
    toggleAuthMode() {
        this.authMode = this.authMode === 'login' ? 'register' : 'login';
        document.getElementById('auth-title').innerText = this.authMode === 'login' ? 'Giriş Yap' : 'Kayıt Ol';
        document.getElementById('auth-switch-text').innerText = this.authMode === 'login' ? 'Hesabın yok mu?' : 'Zaten hesabın var mı?';
        const switchTo = this.authMode === 'login' ? 'Kayıt Ol' : 'Giriş Yap';
        document.querySelector('.auth-switch a').innerText = switchTo;
        
        if(this.authMode === 'register') {
            document.getElementById('group-email').classList.remove('hidden');
        } else {
            document.getElementById('group-email').classList.add('hidden');
        }
    },

    async handleAuth(e) {
        e.preventDefault();
        const username = document.getElementById('auth-username').value;
        const password = document.getElementById('auth-password').value;
        const email = document.getElementById('auth-email').value;

        const endpoint = this.authMode === 'login' ? 'login' : 'register';
        const body = this.authMode === 'login' ? { username, password } : { username, email, password };

        try {
            const res = await fetch(`${API_URL}/Auth/${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });

            if(!res.ok) {
                const err = await res.json();
                // FluentValidation hatalarını yakalama
                if(err.errors) {
                    const firstError = Object.values(err.errors)[0][0];
                    throw new Error(firstError);
                }
                throw new Error(err.message || 'Hata oluştu');
            }

            const data = await res.json();
            
            if(this.authMode === 'register') {
                this.showToast('Başarıyla kayıt olundu! Lütfen giriş yapın.', 'success');
                this.toggleAuthMode();
            } else {
                // Token'ı kaydet
                this.state.token = data.token;
                localStorage.setItem('token', data.token);
                this.decodeUser();
                this.renderNav();
                this.showToast(`Hoş Geldin ${this.state.user.username}!`, 'success');
                this.navigate('home');
            }
        } catch(e) {
            this.showToast(e.message, 'error');
        }
    },

    logout() {
        this.state.token = null;
        this.state.user = null;
        localStorage.removeItem('token');
        
        // Sepeti sıfırla (Sadece kişiye özel sepete geçiş yapıyoruz)
        this.state.cart = [];
        document.getElementById('cart-count').innerText = '0';
        this.renderCart();

        this.renderNav();
        this.navigate('home');
        this.showToast('Çıkış yapıldı.', 'success');
    },

    // ================== ODERS ADMIN ==================
    async loadOrders() {
        try {
            const res = await fetch(`${API_URL}/Orders?PageNumber=1&PageSize=50`, {
                headers: { 'Authorization': `Bearer ${this.state.token}` }
            });
            const data = await res.json();
            const list = document.getElementById('orders-list');
            list.innerHTML = '';
            
            const orders = data.items || data;
            
            orders.forEach(o => {
                list.innerHTML += `
                    <tr>
                        <td>#${o.id}</td>
                        <td>${o.userName} (ID: ${o.userId})</td>
                        <td>${new Date(o.orderDate).toLocaleDateString()}</td>
                        <td style="color: #10b981; font-weight:bold;">${o.totalAmount} TL</td>
                        <td>
                            <button class="btn-primary" onclick='app.viewOrderDetails(${JSON.stringify(o).replace(/'/g, "\\'")})' style="padding: 0.3rem 0.5rem; font-size: 0.8rem;">
                                <i class="fa-solid fa-eye"></i> Detay
                            </button>
                        </td>
                    </tr>
                `;
            });
        } catch(e) {
            this.showToast('Siparişler yüklenemedi (Sadece Admin görebilir)', 'error');
        }
    },

    viewOrderDetails(order) {
        document.getElementById('order-modal-title').innerText = `Sipariş Detayı (#${order.id})`;
        document.getElementById('order-modal-total').innerText = `${order.totalAmount} TL`;
        
        const list = document.getElementById('order-items-list');
        list.innerHTML = '';

        if(order.items && order.items.length > 0) {
            order.items.forEach(item => {
                list.innerHTML += `
                    <tr>
                        <td>#${item.productId}</td>
                        <td>${item.productName}</td>
                        <td>${item.quantity}</td>
                        <td>${item.unitPrice} TL</td>
                        <td style="color: #10b981; font-weight:bold;">${item.totalPrice} TL</td>
                    </tr>
                `;
            });
        } else {
            list.innerHTML = `<tr><td colspan="5" style="text-align:center;">Ürün bulunamadı.</td></tr>`;
        }

        document.getElementById('order-details-modal').classList.remove('hidden');
    },

    closeOrderModal() {
        document.getElementById('order-details-modal').classList.add('hidden');
    },

    // ================== ADMIN PRODUCT MANAGEMENT ==================
    async loadAdminProducts() {
        try {
            document.getElementById('admin-category').value = this.state.adminProductParams.category;
            let url = `${API_URL}/Products?PageSize=200`;
            if (this.state.adminProductParams.category) {
                url += `&Category=${encodeURIComponent(this.state.adminProductParams.category)}`;
            }
            
            const res = await fetch(url);
            const data = await res.json();
            const list = document.getElementById('admin-product-list');
            list.innerHTML = '';
            
            const items = data.items || data;
            items.forEach(p => {
                const imgSource = p.imageUrl 
                    ? (p.imageUrl.startsWith('http') ? p.imageUrl : `https://storehub-alper.azurewebsites.net${p.imageUrl}`) 
                    : null;

                const imgHtml = imgSource
                    ? `<img src="${imgSource}" class="admin-img-preview">` 
                    : `<div class="td-img-placeholder"><i class="fa-solid fa-image"></i></div>`;

                list.innerHTML += `
                    <tr>
                        <td>${imgHtml}</td>
                        <td>${p.name}</td>
                        <td style="font-weight: 500; font-size: 0.9em; opacity: 0.8;">${p.category || 'Belirtilmedi'}</td>
                        <td style="color: #10b981; font-weight:bold;">${p.price} TL</td>
                        <td>${p.stock}</td>
                        <td>
                            <div class="action-btns">
                                <button onclick="app.editProduct(${p.id})" class="btn-secondary btn-edit"><i class="fa-solid fa-pen"></i></button>
                                <button onclick="app.deleteProduct(${p.id})" class="btn-secondary btn-delete"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </td>
                    </tr>
                `;
            });
        } catch(e) {
            this.showToast('Ürünler yüklenemedi.', 'error');
        }
    },

    handleAdminCategoryChange(e) {
        this.state.adminProductParams.category = e.target.value;
        this.loadAdminProducts();
    },

    openProductModal(product = null) {
        const modal = document.getElementById('product-modal');
        const form = document.getElementById('product-form');
        const title = document.getElementById('modal-title');
        
        form.reset();
        document.getElementById('edit-product-id').value = '';
        document.getElementById('image-preview').innerHTML = '';
        
        if(product) {
            title.innerText = 'Ürün Düzenle';
            document.getElementById('edit-product-id').value = product.id;
            document.getElementById('p-name').value = product.name;
            document.getElementById('p-desc').value = product.description;
            document.getElementById('p-price').value = product.price;
            document.getElementById('p-stock').value = product.stock;
            document.getElementById('p-category').value = product.category || '';
            if(product.imageUrl) {
                document.getElementById('image-preview').innerHTML = `<img src="${product.imageUrl.startsWith('http') ? product.imageUrl : 'https://storehub-alper.azurewebsites.net' + product.imageUrl}">`;
            }
        } else {
            title.innerText = 'Yeni Ürün Ekle';
            document.getElementById('p-category').value = 'Genel';
        }
        
        modal.classList.remove('hidden');
    },

    closeProductModal() {
        document.getElementById('product-modal').classList.add('hidden');
    },

    async editProduct(id) {
        try {
            const res = await fetch(`${API_URL}/Products/${id}`);
            const product = await res.json();
            this.openProductModal(product);
        } catch(e) {
            this.showToast('Ürün bilgisi alınamadı.', 'error');
        }
    },

    async saveProduct(e) {
        e.preventDefault();
        const id = document.getElementById('edit-product-id').value;
        const name = document.getElementById('p-name').value;
        const description = document.getElementById('p-desc').value;
        const price = parseFloat(document.getElementById('p-price').value);
        const stock = parseInt(document.getElementById('p-stock').value);
        const category = document.getElementById('p-category').value;
        const imageFile = document.getElementById('p-image').files[0];

        const method = id ? 'PUT' : 'POST';
        const url = id ? `${API_URL}/Products/${id}` : `${API_URL}/Products`;
        const body = { id: id ? parseInt(id) : 0, name, description, price, stock, category };

        try {
            const res = await fetch(url, {
                method,
                headers: { 
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.state.token}`
                },
                body: JSON.stringify(body)
            });

            if(!res.ok) {
                const err = await res.json();
                throw new Error(err.message || 'Ürün kaydedilemedi.');
            }

            let savedProduct;
            if(method === 'POST') {
                savedProduct = await res.json();
            } else {
                // PUT genelde NoContent döner, o yüzden ID'yi koruyoruz
                savedProduct = { id: parseInt(id) };
            }

            // GÖRSEL YÜKLEME (Varsa)
            if(imageFile) {
                await this.uploadImage(savedProduct.id, imageFile);
            }

            this.showToast('Ürün başarıyla kaydedildi!', 'success');
            this.closeProductModal();
            this.loadAdminProducts();
        } catch(e) {
            this.showToast(e.message, 'error');
        }
    },

    async uploadImage(productId, file) {
        const formData = new FormData();
        formData.append('file', file);

        const res = await fetch(`${API_URL}/Products/${productId}/image`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${this.state.token}` },
            body: formData
        });

        if(!res.ok) throw new Error('Görsel yüklenemedi.');
    },

    async deleteProduct(id) {
        if(!confirm('Bu ürünü silmek istediğinize emin misiniz?')) return;

        try {
            const res = await fetch(`${API_URL}/Products/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${this.state.token}` }
            });

            if(!res.ok) throw new Error('Ürün silinemedi.');
            
            this.showToast('Ürün silindi.', 'success');
            this.loadAdminProducts();
        } catch(e) {
            this.showToast(e.message, 'error');
        }
    },

    handleImagePreview(e) {
        const file = e.target.files[0];
        const preview = document.getElementById('image-preview');
        if(file) {
            const reader = new FileReader();
            reader.onload = (ex) => {
                preview.innerHTML = `<img src="${ex.target.result}">`;
            };
            reader.readAsDataURL(file);
        }
    },

    // ================== TOAST ==================
    showToast(message, type) {
        const container = document.getElementById('toast-container');
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `<i class="fa-solid fa-${type === 'success' ? 'check' : 'triangle-exclamation'}"></i> ${message}`;
        container.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }
};

window.addEventListener('DOMContentLoaded', () => {
    app.init();
});
