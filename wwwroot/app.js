// إعدادات النظام
const API_BASE = 'https://alshifaclinic.somee.com/api/Clinic';
let currentUser = null;
let currentSection = 'reception';

// بيانات التخزين المحلي
let localPatients = JSON.parse(localStorage.getItem('patients')) || [];
let localMedications = JSON.parse(localStorage.getItem('medications')) || [];

// دوال المصادقة
async function login() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const department = document.getElementById('department').value;

    if (!username || !password) {
        alert('الرجاء إدخال جميع البيانات');
        return;
    }

    // في بيئة حقيقية، هذا سيتم التحقق من السيرفر
    currentUser = {
        username: username,
        department: department,
        name: getDepartmentName(department)
    };

    document.getElementById('loginScreen').style.display = 'none';
    document.getElementById('mainApp').style.display = 'block';
    document.getElementById('userInfo').textContent = `👤 ${currentUser.name}`;
    
    loadTabs();
    loadSection();
    loadInitialData();
}

function logout() {
    currentUser = null;
    document.getElementById('loginScreen').style.display = 'flex';
    document.getElementById('mainApp').style.display = 'none';
}

// دوال تحميل الواجهة
function loadTabs() {
    const tabs = {
        reception: '📋 الاستقبال',
        doctor: '👨‍⚕️🩺 الطبيب',
        lab: '🧪 المعمل',
        pharmacy: '💊 الصيدلية',
        nurse :' 💉الممرض',
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
    loadSection();
}

async function loadSection() {
    let html = '';
    
    switch(currentSection) {
        case 'reception':
            html = await loadReceptionSection();
            break;
        case 'doctor':
            html = await loadDoctorSection();
            break;
        case 'lab':
            html = await loadLabSection();
            break;
        case 'pharmacy':
            html = await loadPharmacySection();
            break;
        case 'admin':
            html = await loadAdminSection();
            break;
    }
    
    document.getElementById('sectionContent').innerHTML = html;
}

// دوال الأقسام
async function loadReceptionSection() {
    const patients = await fetchPatients();
    
    return `
        <div class="row">
            <div class="col-md-6">
                <div class="card p-3">
                    <h5>تسجيل مريض جديد</h5>
                    <div class="mb-3">
                        <input type="text" id="newPatientName" class="form-control" placeholder="اسم المريض">
                    </div>
                    <div class="mb-3">
                        <input type="number" id="newPatientAge" class="form-control" placeholder="العمر">
                    </div>
                    <div class="mb-3">
                        <input type="text" id="newPatientPhone" class="form-control" placeholder="رقم الهاتف">
                    </div>
                    <button class="btn btn-primary w-100" onclick="registerPatient()">تسجيل المريض</button>
                </div>
            </div>
            <div class="col-md-6">
                <div class="card p-3">
                    <h5>المرضى المسجلين اليوم</h5>
                    <div id="patientsList">
                        ${patients.map(p => `
                            <div class="border p-2 mb-2">
                                <strong>${p.name}</strong> - ${p.age} سنة
                                <br><small>${p.phone} - ${p.status}</small>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        </div>
    `;
}

async function loadDoctorSection() {
    const waitingPatients = await fetchWaitingPatients();
    
    return `
        <div class="row">
            <div class="col-md-4">
                <div class="card p-3">
                    <h5>المرضى بانتظار الكشف</h5>
                    ${waitingPatients.map(p => `
                        <div class="border p-2 mb-2" onclick="selectPatient(${p.id})" style="cursor:pointer;">
                            <strong>${p.name}</strong> - ${p.age} سنة
                        </div>
                    `).join('')}
                </div>
            </div>
            <div class="col-md-8">
                <div class="card p-3">
                    <h5>الروشتة الطبية</h5>
                    <div class="mb-3">
                        <textarea id="prescription" class="form-control" rows="6" placeholder="اكتب الروشتة هنا..."></textarea>
                    </div>
                    <button class="btn btn-success" onclick="savePrescription()">حفظ وإرسال للصيدلية</button>
                </div>
            </div>
        </div>
    `;
}

// دوال الاتصال بالسيرفر
async function fetchPatients() {
    try {
        const response = await fetch(`${API_BASE}/patients`);
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('خطأ في جلب المرضى:', error);
    }
    return localPatients;
}

async function registerPatient() {
    const patient = {
        name: document.getElementById('newPatientName').value,
        age: parseInt(document.getElementById('newPatientAge').value),
        phone: document.getElementById('newPatientPhone').value,
        status: 'waiting_doctor',
        registrationDate: new Date().toISOString()
    };

    try {
        const response = await fetch(`${API_BASE}/patients`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(patient)
        });

        if (response.ok) {
            alert('تم تسجيل المريض بنجاح');
            loadSection();
        }
    } catch (error) {
        // حفظ محلي إذا فشل الاتصال
        patient.id = Date.now();
        localPatients.push(patient);
        localStorage.setItem('patients', JSON.stringify(localPatients));
        alert('تم التسجيل محلياً (يجب مزامنة البيانات لاحقاً)');
        loadSection();
    }
}

async function fetchWaitingPatients() {
    try {
        const response = await fetch(`${API_BASE}/patients/waiting`);
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('خطأ في جلب المرضى المنتظرين:', error);
    }
    return localPatients.filter(p => p.status === 'waiting_doctor');
}

// دوال مساعدة
function getDepartmentName(dept) {
    const names = {
        reception: 'موظف الاستقبال',
        doctor: 'دكتور العيادة',
        lab: 'فني المعمل',
        pharmacy: 'الصيدلي',
        nurse:'الممرض',
        admin: 'مدير النظام'
    };
    return names[dept] || dept;
}

async function loadInitialData() {
    try {
        // جلب الأدوية من السيرفر
        const medsResponse = await fetch(`${API_BASE}/medications`);
        if (medsResponse.ok) {
            localMedications = await medsResponse.json();
            localStorage.setItem('medications', JSON.stringify(localMedications));
        }
    } catch (error) {
        console.log('استخدام البيانات المحلية');
    }
}
