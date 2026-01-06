// ============================================
// نظام مجمع الشفاء - الإصدار السحابي الخالص
// ============================================

// إعدادات النظام
const API_BASE = 'https://alshifaclinic.somee.com/api/Clinic';
let currentUser = null;
let currentSection = 'reception';

// ============================================
// دوال المصادقة والجلسات
// ============================================

async function login() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const department = document.getElementById('department').value;

    if (!username || !password) {
        showAlert('الرجاء إدخال جميع البيانات', 'danger');
        return;
    }

    try {
        showLoader();
        
        const response = await fetch(`${API_BASE}/Auth/Login`, {
            method: 'POST',
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
            throw new Error('فشل في تسجيل الدخول');
        }

        const result = await response.json();
        
        if (result.success) {
            currentUser = {
                ...result.data,
                department: department,
                token: result.token
            };
            
            // تخزين الجلسة
            sessionStorage.setItem('userToken', result.token);
            sessionStorage.setItem('userData', JSON.stringify(currentUser));
            
            showAlert(`مرحباً ${currentUser.fullName || currentUser.username}`, 'success');
            
            // تحميل الواجهة الرئيسية
            document.getElementById('loginScreen').style.display = 'none';
            document.getElementById('mainApp').style.display = 'block';
            document.getElementById('userInfo').textContent = `👤 ${currentUser.fullName || currentUser.username} (${getDepartmentName(department)})`;
            
            loadTabs();
            loadSection();
            
        } else {
            showAlert(result.message || 'اسم المستخدم أو كلمة المرور غير صحيحة', 'danger');
        }
    } catch (error) {
        console.error('خطأ في تسجيل الدخول:', error);
        showAlert('فشل الاتصال بالسيرفر. يرجى التحقق من اتصال الإنترنت', 'danger');
    } finally {
        hideLoader();
    }
}

function logout() {
    if (confirm('هل تريد تسجيل الخروج؟')) {
        // مسح الجلسة
        sessionStorage.removeItem('userToken');
        sessionStorage.removeItem('userData');
        
        currentUser = null;
        document.getElementById('loginScreen').style.display = 'flex';
        document.getElementById('mainApp').style.display = 'none';
        document.getElementById('username').value = '';
        document.getElementById('password').value = '';
        showAlert('تم تسجيل الخروج بنجاح', 'info');
    }
}

// ============================================
// دوال تحميل الواجهة
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
        case 'nurse':
            html = await loadNurseSection();
            break;
        case 'admin':
            html = await loadAdminSection();
            break;
    }
    
    document.getElementById('sectionContent').innerHTML = html;
}

// ============================================
// دوال قسم الاستقبال
// ============================================

async function loadReceptionSection() {
    try {
        const patients = await fetchPatients();
        
        return `
            <div class="row">
                <div class="col-md-6">
                    <div class="card p-3">
                        <h5>تسجيل مريض جديد</h5>
                        <div class="mb-3">
                            <input type="text" id="pName" class="form-control" placeholder="اسم المريض" required>
                        </div>
                        <div class="row mb-3">
                            <div class="col-6">
                                <input type="number" id="pAge" class="form-control" placeholder="العمر" min="0" max="120" required>
                            </div>
                            <div class="col-6">
                                <select id="pGender" class="form-control" required>
                                    <option value="">اختر الجنس</option>
                                    <option value="ذكر">ذكر</option>
                                    <option value="أنثى">أنثى</option>
                                </select>
                            </div>
                        </div>
                        <div class="mb-3">
                            <input type="tel" id="pPhone" class="form-control" placeholder="رقم الهاتف">
                        </div>
                        <div class="mb-3">
                            <input type="text" id="pAddress" class="form-control" placeholder="العنوان">
                        </div>
                        <div class="mb-3">
                            <input type="number" id="pTicket" class="form-control" placeholder="سعر التذكرة" value="2000" min="0" required>
                        </div>
                        <button class="btn btn-primary w-100" onclick="registerPatient()">تسجيل المريض</button>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card p-3">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h5 class="mb-0">المرضى المسجلين اليوم</h5>
                            <button class="btn btn-sm btn-outline-primary" onclick="loadReceptionSectionData()">
                                <i class="fas fa-sync-alt"></i> تحديث
                            </button>
                        </div>
                        <div id="patientsListContainer">
                            ${patients.length > 0 ? patients.slice(0, 10).map(p => `
                                <div class="border p-2 mb-2 patient-card" onclick="viewPatientDetails(${p.id})" style="cursor:pointer;">
                                    <div class="d-flex justify-content-between">
                                        <div>
                                            <strong>${p.name}</strong><br>
                                            <small>${p.age} سنة | ${p.gender || ''} | ${p.phone || ''}</small>
                                        </div>
                                        <div class="text-end">
                                            <small class="text-muted">${formatDate(p.registrationDate)}</small><br>
                                            <span class="badge ${getStatusBadgeClass(p.status)}">
                                                ${getStatusText(p.status)}
                                            </span>
                                        </div>
                                    </div>
                                </div>
                            `).join('') : '<div class="alert alert-info">لا يوجد مرضى مسجلين</div>'}
                        </div>
                        ${patients.length > 10 ? `
                            <div class="text-center mt-3">
                                <button class="btn btn-sm btn-outline-secondary" onclick="showAllPatients()">عرض الكل</button>
                            </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        return `
            <div class="alert alert-danger">
                <h5>خطأ في تحميل بيانات الاستقبال</h5>
                <p>${error.message}</p>
                <button class="btn btn-primary" onclick="loadReceptionSection()">إعادة المحاولة</button>
            </div>
        `;
    }
}

async function loadReceptionSectionData() {
    try {
        const patients = await fetchPatients();
        const container = document.getElementById('patientsListContainer');
        if (!container) return;
        
        if (patients.length > 0) {
            container.innerHTML = patients.slice(0, 10).map(p => `
                <div class="border p-2 mb-2 patient-card" onclick="viewPatientDetails(${p.id})" style="cursor:pointer;">
                    <div class="d-flex justify-content-between">
                        <div>
                            <strong>${p.name}</strong><br>
                            <small>${p.age} سنة | ${p.gender || ''} | ${p.phone || ''}</small>
                        </div>
                        <div class="text-end">
                            <small class="text-muted">${formatDate(p.registrationDate)}</small><br>
                            <span class="badge ${getStatusBadgeClass(p.status)}">
                                ${getStatusText(p.status)}
                            </span>
                        </div>
                    </div>
                </div>
            `).join('');
        } else {
            container.innerHTML = '<div class="alert alert-info">لا يوجد مرضى مسجلين</div>';
        }
    } catch (error) {
        showAlert('فشل تحديث البيانات: ' + error.message, 'danger');
    }
}

// ============================================
// دوال الاتصال بالسيرفر
// ============================================

async function fetchWithAuth(url, options = {}) {
    const token = sessionStorage.getItem('userToken');
    
    const defaultOptions = {
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
            // غير مصرح - إعادة توجيه للدخول
            showAlert('انتهت الجلسة، يرجى تسجيل الدخول مرة أخرى', 'warning');
            logout();
            return null;
        }
        
        if (!response.ok) {
            throw new Error(`خطأ في السيرفر: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('خطأ في الاتصال:', error);
        throw error;
    }
}

async function fetchPatients() {
    try {
        const data = await fetchWithAuth('/Patient/GetAll');
        return data || [];
    } catch (error) {
        console.error('خطأ في جلب المرضى:', error);
        throw new Error('فشل تحميل بيانات المرضى');
    }
}

async function fetchPatientById(id) {
    try {
        return await fetchWithAuth(`/Patient/GetById/${id}`);
    } catch (error) {
        console.error('خطأ في جلب بيانات المريض:', error);
        throw new Error('فشل تحميل بيانات المريض');
    }
}

async function registerPatient() {
    const patient = {
        name: document.getElementById('pName').value,
        age: parseInt(document.getElementById('pAge').value) || 0,
        gender: document.getElementById('pGender').value,
        phone: document.getElementById('pPhone').value || null,
        address: document.getElementById('pAddress').value || null,
        ticketPrice: parseFloat(document.getElementById('pTicket').value) || 2000,
        status: 'waiting_doctor',
        registrationDate: new Date().toISOString(),
        createdBy: currentUser.username
    };

    // التحقق من البيانات
    if (!patient.name || !patient.age || !patient.gender) {
        showAlert('الرجاء إدخال الاسم والعمر والجنس', 'warning');
        return;
    }

    try {
        showLoader();
        
        const result = await fetchWithAuth('/Patient/Create', {
            method: 'POST',
            body: JSON.stringify(patient)
        });

        if (result && result.success) {
            showAlert('تم تسجيل المريض بنجاح', 'success');
            
            // مسح النموذج
            ['pName', 'pAge', 'pPhone', 'pAddress', 'pTicket'].forEach(id => {
                document.getElementById(id).value = '';
            });
            document.getElementById('pGender').value = '';
            
            // تحديث القائمة
            loadReceptionSectionData();
            
            // طباعة التذكرة
            if (confirm('هل تريد طباعة تذكرة المريض؟')) {
                const newPatient = result.data || { ...patient, id: result.id };
                printTicket(newPatient);
            }
        } else {
            throw new Error(result?.message || 'فشل تسجيل المريض');
        }
    } catch (error) {
        showAlert('خطأ في تسجيل المريض: ' + error.message, 'danger');
    } finally {
        hideLoader();
    }
}

async function fetchWaitingPatients() {
    try {
        const data = await fetchWithAuth('/Patient/GetWaiting');
        return data || [];
    } catch (error) {
        console.error('خطأ في جلب المرضى المنتظرين:', error);
        throw new Error('فشل تحميل قائمة الانتظار');
    }
}

async function fetchMedications() {
    try {
        const data = await fetchWithAuth('/Medication/GetAll');
        return data || [];
    } catch (error) {
        console.error('خطأ في جلب الأدوية:', error);
        throw new Error('فشل تحميل قائمة الأدوية');
    }
}

async function fetchStatistics() {
    try {
        const data = await fetchWithAuth('/Admin/GetStatistics');
        return data || {};
    } catch (error) {
        console.error('خطأ في جلب الإحصائيات:', error);
        return {};
    }
}

// ============================================
// دوال الأقسام الأخرى
// ============================================

async function loadDoctorSection() {
    try {
        const waitingPatients = await fetchWaitingPatients();
        
        return `
            <div class="row">
                <div class="col-md-4">
                    <div class="card p-3">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h5 class="mb-0">المرضى بانتظار الكشف</h5>
                            <span class="badge bg-primary">${waitingPatients.length}</span>
                        </div>
                        <div id="doctorQueue">
                            ${waitingPatients.length > 0 ? waitingPatients.map(p => `
                                <div class="border p-2 mb-2" onclick="selectPatientForDoctor(${p.id}, '${p.name}')" 
                                     style="cursor:pointer; ${window.selectedPatientId === p.id ? 'background-color: #e3f2fd;' : ''}">
                                    <div class="d-flex justify-content-between align-items-center">
                                        <div>
                                            <strong>${p.name}</strong><br>
                                            <small>${p.age} سنة | ${p.gender}</small>
                                        </div>
                                        <span class="badge bg-warning">بانتظار الطبيب</span>
                                    </div>
                                </div>
                            `).join('') : '<div class="alert alert-info">لا يوجد مرضى بانتظار الكشف</div>'}
                        </div>
                    </div>
                </div>
                <div class="col-md-8">
                    <div class="card p-3">
                        <h5>الروشتة الطبية</h5>
                        <div class="mb-3">
                            <label>المريض المحدد</label>
                            <input type="text" id="selectedPatientName" class="form-control" readonly 
                                   value="${window.selectedPatientName || 'لم يتم اختيار مريض'}">
                        </div>
                        <div class="mb-3">
                            <label>الشكوى الرئيسية</label>
                            <textarea id="complaint" class="form-control" rows="2" placeholder="الشكوى الرئيسية للمريض..."></textarea>
                        </div>
                        <div class="mb-3">
                            <label>التشخيص</label>
                            <textarea id="diagnosis" class="form-control" rows="3" placeholder="التشخيص الطبي..."></textarea>
                        </div>
                        <div class="mb-3">
                            <label>العلاج والروشتة</label>
                            <textarea id="prescription" class="form-control" rows="4" placeholder="اكتب الروشتة الطبية هنا..."></textarea>
                        </div>
                        <div class="mb-3">
                            <label>ملاحظات الطبيب</label>
                            <textarea id="doctorNotes" class="form-control" rows="2" placeholder="ملاحظات إضافية..."></textarea>
                        </div>
                        <button class="btn btn-success" onclick="savePrescription()" 
                                ${!window.selectedPatientId ? 'disabled' : ''}>
                            حفظ وإرسال للصيدلية
                        </button>
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        return `
            <div class="alert alert-danger">
                <h5>خطأ في تحميل قسم الطبيب</h5>
                <p>${error.message}</p>
                <button class="btn btn-primary" onclick="loadDoctorSection()">إعادة المحاولة</button>
            </div>
        `;
    }
}

function selectPatientForDoctor(patientId, patientName) {
    window.selectedPatientId = patientId;
    window.selectedPatientName = patientName;
    loadDoctorSection();
}

async function savePrescription() {
    if (!window.selectedPatientId) {
        showAlert('الرجاء اختيار مريض أولاً', 'warning');
        return;
    }

    const prescriptionData = {
        patientId: window.selectedPatientId,
        doctorId: currentUser.id || currentUser.username,
        doctorName: currentUser.fullName || currentUser.username,
        complaint: document.getElementById('complaint').value,
        diagnosis: document.getElementById('diagnosis').value,
        prescription: document.getElementById('prescription').value,
        notes: document.getElementById('doctorNotes').value,
        date: new Date().toISOString()
    };

    try {
        showLoader();
        
        const result = await fetchWithAuth('/Prescription/Create', {
            method: 'POST',
            body: JSON.stringify(prescriptionData)
        });

        if (result && result.success) {
            showAlert('تم حفظ الروشتة بنجاح', 'success');
            
            // مسح الحقول
            ['complaint', 'diagnosis', 'prescription', 'doctorNotes'].forEach(id => {
                document.getElementById(id).value = '';
            });
            
            // إزالة الاختيار
            window.selectedPatientId = null;
            window.selectedPatientName = null;
            
            // تحديث القائمة
            loadDoctorSection();
        } else {
            throw new Error(result?.message || 'فشل حفظ الروشتة');
        }
    } catch (error) {
        showAlert('خطأ في حفظ الروشتة: ' + error.message, 'danger');
    } finally {
        hideLoader();
    }
}

// ============================================
// دوال المساعدة العامة
// ============================================

function showAlert(message, type = 'info') {
    // إزالة أي تنبيهات سابقة
    const oldAlerts = document.querySelectorAll('.alert-fixed');
    oldAlerts.forEach(alert => alert.remove());
    
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show alert-fixed`;
    alertDiv.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 500px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // إزالة التنبيه تلقائياً بعد 5 ثواني
    setTimeout(() => {
        if (alertDiv.parentElement) {
            alertDiv.remove();
        }
    }, 5000);
}

function showLoader() {
    let loader = document.getElementById('loader');
    if (!loader) {
        loader = document.createElement('div');
        loader.id = 'loader';
        loader.className = 'loader-overlay';
        loader.innerHTML = `
            <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status">
                <span class="visually-hidden">جاري التحميل...</span>
            </div>
        `;
        document.body.appendChild(loader);
    }
    loader.style.display = 'flex';
}

function hideLoader() {
    const loader = document.getElementById('loader');
    if (loader) {
        loader.style.display = 'none';
    }
}

function getDepartmentName(dept) {
    const names = {
        reception: 'موظف الاستقبال',
        doctor: 'دكتور العيادة',
        lab: 'فني المعمل',
        pharmacy: 'الصيدلي',
        nurse: 'الممرض',
        admin: 'مدير النظام'
    };
    return names[dept] || dept;
}

function getStatusText(status) {
    const statusMap = {
        'waiting_doctor': 'بانتظار الطبيب',
        'waiting_lab': 'بانتظار المعمل',
        'waiting_pharmacy': 'بانتظار الصيدلية',
        'waiting_nurse': 'بانتظار التمريض',
        'completed': 'مكتمل',
        'in_progress': 'قيد المعالجة',
        'cancelled': 'ملغي'
    };
    return statusMap[status] || status;
}

function getStatusBadgeClass(status) {
    const classMap = {
        'waiting_doctor': 'bg-warning',
        'waiting_lab': 'bg-info',
        'waiting_pharmacy': 'bg-primary',
        'waiting_nurse': 'bg-secondary',
        'completed': 'bg-success',
        'in_progress': 'bg-warning',
        'cancelled': 'bg-danger'
    };
    return classMap[status] || 'bg-secondary';
}

function formatDate(dateString) {
    if (!dateString) return 'غير محدد';
    try {
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return 'غير محدد';
        
        return date.toLocaleDateString('ar-EG', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (error) {
        return 'غير محدد';
    }
}

function printTicket(patient) {
    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <!DOCTYPE html>
        <html dir="rtl">
        <head>
            <title>تذكرة مجمع الشفاء - ${patient.name}</title>
            <meta charset="UTF-8">
            <style>
                * { margin: 0; padding: 0; box-sizing: border-box; }
                body { font-family: 'Arial', sans-serif; padding: 20px; background: #f5f5f5; }
                .ticket { 
                    max-width: 500px; 
                    margin: 0 auto; 
                    background: white; 
                    border: 3px solid #2c7a7b; 
                    border-radius: 15px; 
                    padding: 25px; 
                    box-shadow: 0 5px 15px rgba(0,0,0,0.1);
                }
                .header { 
                    text-align: center; 
                    margin-bottom: 25px; 
                    padding-bottom: 15px; 
                    border-bottom: 2px dashed #2c7a7b; 
                }
                .header h2 { color: #2c7a7b; margin-bottom: 5px; }
                .header h4 { color: #555; }
                .info { margin: 15px 0; }
                .info p { margin: 8px 0; padding: 8px; background: #f9f9f9; border-radius: 5px; }
                .info strong { color: #2c7a7b; display: inline-block; width: 140px; }
                .footer { 
                    margin-top: 25px; 
                    text-align: center; 
                    padding-top: 15px; 
                    border-top: 2px dashed #2c7a7b; 
                    color: #666; 
                    font-size: 14px;
                }
                .barcode { 
                    text-align: center; 
                    margin: 20px 0; 
                    padding: 10px; 
                    background: #f0f0f0; 
                    font-family: monospace; 
                    letter-spacing: 3px;
                }
                @media print {
                    body { background: white; }
                    .ticket { border: none; box-shadow: none; }
                    button { display: none; }
                }
            </style>
        </head>
        <body>
            <div class="ticket">
                <div class="header">
                    <h2>🏥 مجمع الشفاء الطبي</h2>
                    <h4>تذكرة مريض</h4>
                </div>
                <div class="barcode">
                    ${patient.id.toString().split('').join(' ')}
                </div>
                <div class="info">
                    <p><strong>اسم المريض:</strong> ${patient.name || 'غير معروف'}</p>
                    <p><strong>رقم الملف:</strong> ${patient.id || 'غير معروف'}</p>
                    <p><strong>العمر:</strong> ${patient.age || 'غير معروف'} سنة</p>
                    <p><strong>الجنس:</strong> ${patient.gender || 'غير معروف'}</p>
                    <p><strong>التاريخ:</strong> ${formatDate(patient.registrationDate || new Date())}</p>
                    <p><strong>سعر التذكرة:</strong> ${patient.ticketPrice || 2000} ج.س</p>
                    <p><strong>الحالة:</strong> ${getStatusText(patient.status || 'waiting_doctor')}</p>
                </div>
                <div class="footer">
                    <p>شكراً لزيارتكم - نتمنى لكم الشفاء العاجل</p>
                    <p>📞 للاستفسار: 0912345678</p>
                    <p>📍 العنوان: الخرطوم - السوق العربي</p>
                    <p style="margin-top: 10px; color: #999; font-size: 12px;">
                        ${new Date().toLocaleString('ar-EG')}
                    </p>
                </div>
            </div>
            <div style="text-align: center; margin-top: 20px;">
                <button onclick="window.print()" style="padding: 10px 20px; background: #2c7a7b; color: white; border: none; border-radius: 5px; cursor: pointer;">
                    🖨️ طباعة التذكرة
                </button>
            </div>
        </body>
        </html>
    `);
    printWindow.document.close();
}

function viewPatientDetails(patientId) {
    showLoader();
    fetchPatientById(patientId)
        .then(patient => {
            hideLoader();
            if (patient) {
                const modalContent = `
                    <div class="modal fade" id="patientModal" tabindex="-1">
                        <div class="modal-dialog modal-lg">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">تفاصيل المريض: ${patient.name}</h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <p><strong>الرقم:</strong> ${patient.id}</p>
                                            <p><strong>العمر:</strong> ${patient.age} سنة</p>
                                            <p><strong>الجنس:</strong> ${patient.gender}</p>
                                            <p><strong>الهاتف:</strong> ${patient.phone || 'غير محدد'}</p>
                                        </div>
                                        <div class="col-md-6">
                                            <p><strong>العنوان:</strong> ${patient.address || 'غير محدد'}</p>
                                            <p><strong>سعر التذكرة:</strong> ${patient.ticketPrice} ج.س</p>
                                            <p><strong>الحالة:</strong> <span class="badge ${getStatusBadgeClass(patient.status)}">${getStatusText(patient.status)}</span></p>
                                            <p><strong>تاريخ التسجيل:</strong> ${formatDate(patient.registrationDate)}</p>
                                        </div>
                                    </div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">إغلاق</button>
                                    <button type="button" class="btn btn-primary" onclick="printTicket(${JSON.stringify(patient).replace(/"/g, '&quot;')})">طباعة التذكرة</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                // إضافة المودال إلى الصفحة
                const modalContainer = document.getElementById('modalContainer') || (() => {
                    const div = document.createElement('div');
                    div.id = 'modalContainer';
                    document.body.appendChild(div);
                    return div;
                })();
                modalContainer.innerHTML = modalContent;
                
                // عرض المودال
                const modal = new bootstrap.Modal(document.getElementById('patientModal'));
                modal.show();
            }
        })
        .catch(error => {
            hideLoader();
            showAlert('فشل تحميل بيانات المريض: ' + error.message, 'danger');
        });
}

// ============================================
// تهيئة النظام
// ============================================

function initSystem() {
    // إضافة أنماط CSS
    const style = document.createElement('style');
    style.textContent = `
        .loader-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.9);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 9999;
            backdrop-filter: blur(3px);
        }
        .patient-card {
            cursor: pointer;
            transition: all 0.3s ease;
            border-left: 4px solid transparent !important;
        }
        .patient-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            border-left-color: #2c7a7b !important;
        }
        .nav-link {
            cursor: pointer;
            font-weight: 500;
            transition: all 0.3s ease;
        }
        .nav-link:hover {
            background-color: rgba(44, 122, 123, 0.1);
        }
        .nav-link.active {
            background-color: #2c7a7b !important;
            color: white !important;
        }
        #loginScreen {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
        }
        .login-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }
        #mainApp {
            background: #f8f9fa;
            min-height: 100vh;
        }
    `;
    document.head.appendChild(style);
    
    // التحقق من الجلسة المخزنة
    const savedToken = sessionStorage.getItem('userToken');
    const savedUser = sessionStorage.getItem('userData');
    
    if (savedToken && savedUser) {
        try {
            currentUser = JSON.parse(savedUser);
            document.getElementById('loginScreen').style.display = 'none';
            document.getElementById('mainApp').style.display = 'block';
            document.getElementById('userInfo').textContent = `👤 ${currentUser.fullName || currentUser.username} (${getDepartmentName(currentUser.department)})`;
            loadTabs();
            loadSection();
        } catch (error) {
            sessionStorage.clear();
        }
    }
    
    // تحديث الساعة
    updateClock();
    setInterval(updateClock, 1000);
}

function updateClock() {
    const now = new Date();
    const clockElement = document.getElementById('clockDisplay');
    if (clockElement) {
        const options = {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: true
        };
        clockElement.textContent = now.toLocaleTimeString('ar-EG', options);
    }
}

// ============================================
// تشغيل النظام
// ============================================

window.addEventListener('DOMContentLoaded', initSystem);

// دوال الأقسام الأخرى (نموذجية)
async function loadLabSection() {
    return `
        <div class="card">
            <div class="card-body">
                <h5>🧪 قسم المعمل</h5>
                <p class="text-muted">هذا القسم تحت التطوير</p>
            </div>
        </div>
    `;
}

async function loadPharmacySection() {
    return `
        <div class="card">
            <div class="card-body">
                <h5>💊 قسم الصيدلية</h5>
                <p class="text-muted">هذا القسم تحت التطوير</p>
            </div>
        </div>
    `;
}

async function loadNurseSection() {
    return `
        <div class="card">
            <div class="card-body">
                <h5>💉 قسم التمريض</h5>
                <p class="text-muted">هذا القسم تحت التطوير</p>
            </div>
        </div>
    `;
}

async function loadAdminSection() {
    try {
        const stats = await fetchStatistics();
        return `
            <div class="row">
                <div class="col-md-3 mb-3">
                    <div class="card bg-primary text-white">
                        <div class="card-body text-center">
                            <h5>إجمالي المرضى</h5>
                            <h2>${stats.totalPatients || 0}</h2>
                        </div>
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <div class="card bg-success text-white">
                        <div class="card-body text-center">
                            <h5>المكتملين اليوم</h5>
                            <h2>${stats.completedToday || 0}</h2>
                        </div>
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <div class="card bg-warning text-white">
                        <div class="card-body text-center">
                            <h5>قيد الانتظار</h5>
                            <h2>${stats.waiting || 0}</h2>
                        </div>
                    </div>
                </div>
                <div class="col-md-3 mb-3">
                    <div class="card bg-info text-white">
                        <div class="card-body text-center">
                            <h5>الإيرادات اليوم</h5>
                            <h2>${stats.todayRevenue || 0} ج.س</h2>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card mt-4">
                <div class="card-body">
                    <h5>📊 لوحة التحكم الإدارية</h5>
                    <div class="mt-3">
                        <button class="btn btn-outline-primary" onclick="generateReport('daily')">تقرير يومي</button>
                        <button class="btn btn-outline-success" onclick="generateReport('weekly')">تقرير أسبوعي</button>
                        <button class="btn btn-outline-info" onclick="generateReport('monthly')">تقرير شهري</button>
                    </div>
                </div>
            </div>
        `;
    } catch (error) {
        return `
            <div class="alert alert-danger">
                <h5>خطأ في تحميل قسم الإدارة</h5>
                <p>${error.message}</p>
            </div>
        `;
    }
}

// وظيفة مساعدة لتوليد التقارير
async function generateReport(type) {
    showAlert(`جارٍ إنشاء التقرير ${type}...`, 'info');
    // يمكن تنفيذ توليد التقارير هنا
}
