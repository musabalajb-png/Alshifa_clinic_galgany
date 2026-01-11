// ============================================
// نظام مجمع الشفاء - نسخة السيرفر المحلي (تلفون مصعب)
// ============================================

// 1. إعدادات العنوان - استبدل الـ IP بـ IP تلفونك في Tailscale
const TAILSCALE_IP = '100.x.y.z'; // <--- ضع الـ IP هنا
const API_BASE = `http://${TAILSCALE_IP}:5000/api/Clinic`; 

let currentUser = null;
let currentSection = 'reception';

// دالة فحص الاتصال بالسيرفر المحلي
async function checkLocalServer() {
    try {
        const res = await fetch(`${API_BASE}/Auth/Status`, { method: 'GET' });
        document.getElementById('onlineBadge').className = "badge bg-success";
        document.getElementById('onlineBadge').textContent = "متصل بالسيرفر ✅";
    } catch (e) {
        document.getElementById('onlineBadge').className = "badge bg-danger";
        document.getElementById('onlineBadge').textContent = "السيرفر غير متاح ❌";
    }
}

// ============================================
// دوال المصادقة (المعدلة للعمل المحلي)
// ============================================

async function login() {
    const username = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value;
    const department = document.getElementById('department').value;

    if (!username || !password) {
        showAlert('الرجاء إدخال اسم المستخدم وكلمة المرور', 'warning');
        return;
    }

    try {
        showLoader();
        
        // تعديل الـ Fetch ليتناسب مع الاتصال المحلي بـ Tailscale
        const response = await fetch(`${API_BASE}/Auth/Login`, {
            method: 'POST',
            mode: 'cors', // السماح بالاتصال عبر الشبكة المحلية
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({
                Username: username,
                Password: password,
                Department: department
            })
        });

        const result = await response.json();

        if (response.ok && result.success) {
            currentUser = {
                ...result.data,
                department: department,
                token: result.token
            };
            
            sessionStorage.setItem('userToken', result.token);
            sessionStorage.setItem('userData', JSON.stringify(currentUser));
            
            // إخفاء شاشة الدخول وعرض السيستم
            document.getElementById('loginScreen').style.display = 'none';
            document.getElementById('mainApp').style.display = 'block';
            document.getElementById('userInfo').textContent = `👤 ${currentUser.fullName || currentUser.username}`;
            
            loadTabs();
            loadSection();
            showAlert('تم تسجيل الدخول بنجاح', 'success');
        } else {
            showAlert(result.message || 'خطأ في بيانات الدخول', 'danger');
        }
    } catch (error) {
        console.error('Login Error:', error);
        showAlert('تعذر الوصول للسيرفر. تأكد أن تطبيق Tailscale شغال في تلفونك والجهاز الحالي', 'danger');
    } finally {
        hideLoader();
    }
}

// ============================================
// تعديل دالة fetchWithAuth للتعامل مع الـ CORS
// ============================================

async function fetchWithAuth(url, options = {}) {
    const token = sessionStorage.getItem('userToken');
    
    const defaultOptions = {
        mode: 'cors', // مهم جداً للربط بين الأجهزة
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
    };
    
    const finalOptions = { ...defaultOptions, ...options };
    
    try {
        const response = await fetch(`${API_BASE}${url}`, finalOptions);
        if (response.status === 401) { logout(); return null; }
        return await response.json();
    } catch (error) {
        console.error('Network Error:', error);
        throw error;
    }
}
