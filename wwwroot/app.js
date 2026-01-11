// ============================================
// نظام مجمع الشفاء - المحرك الرئيسي
// المطور: مصعب | السيرفر: UserLAnd (100.82.139.35)
// ============================================

const TAILSCALE_IP = '100.82.139.35'; 
const API_BASE = `http://${TAILSCALE_IP}:5000/api/Clinic`; 

let currentUser = null;
let currentSection = 'reception';

// فحص الاتصال بالسيرفر في تلفونك
async function checkServerConnection() {
    try {
        const response = await fetch(`${API_BASE}/Auth/Status`);
        if (response.ok) {
            document.getElementById('onlineBadge').classList.remove('d-none');
            document.getElementById('offlineBadge').classList.add('d-none');
        }
    } catch (e) {
        document.getElementById('onlineBadge').classList.add('d-none');
        document.getElementById('offlineBadge').classList.remove('d-none');
    }
}

// تسجيل الدخول
async function login() {
    const user = document.getElementById('username').value;
    const pass = document.getElementById('password').value;
    const dept = document.getElementById('department').value;

    if (!user || !pass) {
        alert('يا مصعب، الرجاء إدخال البيانات كاملة');
        return;
    }

    showLoader();
    try {
        const response = await fetch(`${API_BASE}/Auth/Login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Username: user, Password: pass, Department: dept })
        });

        const result = await response.json();
        if (result.success) {
            currentUser = { username: user, department: dept, token: result.token };
            sessionStorage.setItem('userData', JSON.stringify(currentUser));
            
            document.getElementById('loginScreen').classList.add('d-none');
            document.getElementById('mainApp').classList.remove('d-none');
            document.getElementById('userInfo').textContent = `👤 ${user}`;
            
            loadTabs();
            switchSection(dept === 'admin' ? 'reception' : dept);
        }
    } catch (error) {
        alert('فشل الاتصال بتلفون السيرفر! تأكد أن تطبيق UserLAnd شغال والـ IP صح.');
    } finally {
        hideLoader();
    }
}

function loadTabs() {
    const tabs = { reception: '📋 الاستقبال', doctor: '👨‍⚕️ العيادة', lab: '🧪 المعمل', pharmacy: '💊 الصيدلية' };
    let html = '';
    for (const [key, value] of Object.entries(tabs)) {
        if (currentUser.department === 'admin' || key === currentUser.department) {
            html += `<li class="nav-item">
                <button class="nav-link w-100 text-end btn ${key === currentSection ? 'active' : ''}" 
                        onclick="switchSection('${key}')">${value}</button>
            </li>`;
        }
    }
    document.getElementById('mainTabs').innerHTML = html;
}

async function switchSection(section) {
    currentSection = section;
    loadTabs();
    const container = document.getElementById('sectionContent');
    
    // بناء الواجهات بناءً على القسم
    if(section === 'reception') {
        container.innerHTML = `
            <div class="card p-4 shadow-sm">
                <h4><i class="fas fa-user-plus"></i> تسجيل مريض جديد</h4>
                <div class="row g-3 mt-2">
                    <div class="col-md-6"><input type="text" id="pName" class="form-control" placeholder="اسم المريض"></div>
                    <div class="col-md-4"><input type="number" id="pAge" class="form-control" placeholder="العمر"></div>
                    <div class="col-md-2"><button class="btn btn-success w-100">حفظ</button></div>
                </div>
            </div>`;
    } else {
        container.innerHTML = `<div class="alert alert-info">قسم ${section} قيد التطوير والربط...</div>`;
    }
}

function logout() {
    sessionStorage.clear();
    location.reload();
}

function showLoader() { document.getElementById('loader').style.display = 'flex'; }
function hideLoader() { document.getElementById('loader').style.display = 'none'; }

// تشغيل الفحص عند الفتح
window.onload = () => {
    checkServerConnection();
    setInterval(checkServerConnection, 10000);
};
