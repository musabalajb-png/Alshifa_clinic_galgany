// ============================================
// نظام مجمع الشفاء - الإصدار الاحترافي (Local Server Mode)
// المطور: مصعب - عيادة الشفاء
// ============================================

// 1. إعدادات الشبكة (Tailscale)
// تأكد من وضع الـ IP الصحيح من تطبيق Tailscale في هاتفك
const TAILSCALE_IP = '100.x.y.z'; 
const API_BASE = `http://${TAILSCALE_IP}:5000/api/Clinic`; 

let currentUser = null;
let currentSection = 'reception';
let selectedPatientId = null;
let selectedPatientName = null;

// ============================================
// إدارة حالة الاتصال (Status Manager)
// ============================================
async function checkServerConnection() {
    const onlineBadge = document.getElementById('onlineBadge');
    const offlineBadge = document.getElementById('offlineBadge');
    
    try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 3000); // 3 ثواني مهلة
        
        const response = await fetch(`${API_BASE}/Auth/Status`, { 
            method: 'GET', 
            mode: 'cors',
            signal: timeoutId.signal 
        });

        if (response.ok) {
            if(onlineBadge) onlineBadge.classList.remove('d-none');
            if(offlineBadge) offlineBadge.classList.add('d-none');
            return true;
        }
    } catch (e) {
        if(onlineBadge) onlineBadge.classList.add('d-none');
        if(offlineBadge) offlineBadge.classList.remove('d-none');
        return false;
    }
    return false;
}

// ============================================
// دوال المصادقة والدخول (Auth)
// ============================================
async function login() {
    const usernameField = document.getElementById('username');
    const passwordField = document.getElementById('password');
    const deptField = document.getElementById('department');

    const username = usernameField.value.trim();
    const password = passwordField.value;
    const department = deptField.value;

    if (!username || !password) {
        showAlert('يا مصعب، الرجاء إدخال جميع البيانات', 'warning');
        return;
    }

    try {
        showLoader();
        
        const response = await fetch(`${API_BASE}/Auth/Login`, {
            method: 'POST',
            mode: 'cors',
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

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error('بيانات الدخول غير صحيحة أو السيرفر غير مستجيب');
        }

        const result = await response.json();
        
        if (result.success) {
            currentUser = {
                ...result.data,
                department: department,
                token: result.token
            };
            
            // تخزين آمن للجلسة
            sessionStorage.setItem('userToken', result.token);
            sessionStorage.setItem('userData', JSON.stringify(currentUser));
            
            showAlert(`مرحباً بك في نظام الشفاء، ${currentUser.fullName || currentUser.username}`, 'success');
            
            // تحويل الواجهة
            document.getElementById('loginScreen').style.display = 'none';
            document.getElementById('mainApp').style.display = 'block';
            document.getElementById('userInfo').textContent = `👤 ${currentUser.fullName || currentUser.username} (${getDepartmentName(department)})`;
            
            loadTabs();
            switchSection(department === 'admin' ? 'reception' : department);
            
        } else {
            showAlert(result.message || 'فشل تسجيل الدخول', 'danger');
        }
    } catch (error) {
        console.error('Login Error:', error);
        showAlert('فشل الاتصال بتلفون السيرفر. تأكد من Tailscale وبورت 5000', 'danger');
    } finally {
        hideLoader();
    }
}

// ============================================
// المعاملات الموثقة (Authenticated Fetch)
// ============================================
async function fetchWithAuth(url, options = {}) {
    const token = sessionStorage.getItem('userToken');
    
    const defaultOptions = {
        mode: 'cors',
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
    };
    
    const finalOptions = { ...defaultOptions, ...options };
    
    try {
        const response = await fetch(`${API_BASE}${url}`, finalOptions);
        
        if (response.status === 401) {
            showAlert('انتهت الجلسة، يرجى الدخول مرة أخرى', 'warning');
            logout();
            return null;
        }
        
        if (!response.ok) throw new Error(`خطأ سيرفر: ${response.status}`);
        
        return await response.json();
    } catch (error) {
        console.error('Fetch Error:', error);
        throw error;
    }
}

// ============================================
// إدارة الأقسام والتبويبات
// ============================================
function loadTabs() {
    const tabs = {
        reception: '📋 الاستقبال',
        doctor: '👨‍⚕️ العيادة',
        lab: '🧪 المعمل',
        pharmacy: '💊 الصيدلية',
        nurse: '💉 الممرض',
        admin: '📊 الإدارة'
    };

    let html = '';
    for (const [key, value] of Object.entries(tabs)) {
        if (currentUser.department === 'admin' || key === currentUser.department) {
            html += `<li class="nav-item">
                <button class="nav-link ${key === currentSection ? 'active' : ''}" 
                        onclick="switchSection('${key}')">${value}</button>
            </li>`;
        }
    }
    document.getElementById('mainTabs').innerHTML = html;
}

function switchSection(section) {
    currentSection = section;
    loadTabs();
    loadSectionContent();
}

async function loadSectionContent() {
    const container = document.getElementById('sectionContent');
    showLoader();
    
    try {
        let html = '';
        switch(currentSection) {
            case 'reception': html = await loadReceptionUI(); break;
            case 'doctor': html = await loadDoctorUI(); break;
            case 'pharmacy': html = await loadPharmacyUI(); break;
            default: html = `<div class="p-5 text-center"><h5>القسم (${currentSection}) قيد التجهيز</h5></div>`;
        }
        container.innerHTML = html;
    } catch (e) {
        container.innerHTML = `<div class="alert alert-danger">فشل تحميل القسم. تأكد من اتصال Tailscale</div>`;
    } finally {
        hideLoader();
    }
}

// ============================================
// مساعدات الواجهة (UI Helpers)
// ============================================
function showAlert(message, type = 'info') {
    const alertBox = document.createElement('div');
    alertBox.className = `alert alert-${type} alert-fixed animate__animated animate__fadeInDown`;
    alertBox.style.cssText = "position:fixed; top:20px; right:20px; z-index:9999; min-width:300px; box-shadow:0 4px 15px rgba(0,0,0,0.2);";
    alertBox.innerHTML = `${message} <button class="btn-close float-start" onclick="this.parentElement.remove()"></button>`;
    document.body.appendChild(alertBox);
    setTimeout(() => alertBox.remove(), 4000);
}

function showLoader() { document.getElementById('loader').style.display = 'flex'; }
function hideLoader() { document.getElementById('loader').style.display = 'none'; }

function getDepartmentName(dept) {
    const map = { reception: 'الاستقبال', doctor: 'الطبيب', lab: 'المعمل', pharmacy: 'الصيدلية', admin: 'المدير' };
    return map[dept] || dept;
}

// ============================================
// التشغيل عند البداية
// ============================================
window.addEventListener('DOMContentLoaded', () => {
    // استعادة الجلسة
    const savedUser = sessionStorage.getItem('userData');
    if (savedUser) {
        currentUser = JSON.parse(savedUser);
        document.getElementById('loginScreen').style.display = 'none';
        document.getElementById('mainApp').style.display = 'block';
        document.getElementById('userInfo').textContent = `👤 ${currentUser.fullName}`;
        loadTabs();
        loadSectionContent();
    }
    
    // فحص الاتصال دورياً
    checkServerConnection();
    setInterval(checkServerConnection, 10000);
});
