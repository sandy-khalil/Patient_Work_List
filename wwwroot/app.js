// ============================================================
// Patient Worklist — frontend logic (Bootstrap 5 + DataTables)
// ============================================================

const API = {
  patients: '/api/patients',
  doctors: '/api/doctors',
  studies: '/api/studies'
};

// ---------------- helpers ----------------

function apiFetch(url, method, body) {
  const opts = { method, headers: { 'Content-Type': 'application/json' } };
  if (body !== undefined) opts.body = JSON.stringify(body);
  return fetch(url, opts).then(async (res) => {
    if (!res.ok) {
      let data = null;
      try { data = await res.json(); } catch (e) { /* ignore */ }
      let msg = 'Request failed (' + res.status + ')';
      if (data) {
        if (typeof data === 'string' && data) msg = data;
        else if (data.message) msg = data.message;
        else if (data.errors && typeof data.errors === 'object') {
          msg = Object.values(data.errors).flat().filter(Boolean).join(' ');
        }
      }
      throw new Error(msg || 'Unknown error');
    }
    return res.status === 204 ? null : res.json();
  });
}

function showToast(message, type) {
  const toastEl = document.getElementById('appToast');
  toastEl.className = 'toast align-items-center border-0 text-bg-' + (type || 'success');
  document.getElementById('appToastBody').textContent = message;
  bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 3500 }).show();
}

function escapeHtml(value) {
  if (value === null || value === undefined) return '';
  return String(value).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

function formatDate(value) {
  if (!value) return '-';
  var d = new Date(value);
  if (isNaN(d.getTime())) return String(value);
  return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
}

function fullName(first, last) {
  return ((first || '') + ' ' + (last || '')).trim() || '-';
}

function statusBadge(status) {
  var map = {
    'Active': 'success', 'Pending': 'warning', 'Inactive': 'secondary',
    'Discharged': 'info', 'Scheduled': 'info', 'In Progress': 'primary',
    'Completed': 'success', 'Reported': 'secondary', 'Archived': 'dark',
    'Cancelled': 'danger'
  };
  return '<span class="badge bg-' + (map[status] || 'secondary') + '">' + escapeHtml(status) + '</span>';
}

function todayStr() {
  var t = new Date();
  return t.getFullYear() + '-' + String(t.getMonth() + 1).padStart(2, '0') + '-' + String(t.getDate()).padStart(2, '0');
}

// Date/time in nav
(function updateDateTime() {
  var el = document.getElementById('navDateTime');
  if (!el) return;
  function tick() {
    var now = new Date();
    el.textContent = now.toLocaleDateString('en-US', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' }) +
      ' ' + now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
  tick();
  setInterval(tick, 30000);
})();

// ---------------- DataTables ----------------

var patientsTable = new DataTable('#patientsTable', {
  ajax: { url: API.patients, dataSrc: '' },
  columns: [
    { data: 'patientId' },
    { data: null, render: function(d) { return escapeHtml(fullName(d.firstName, d.lastName)); } },
    { data: 'mrn', render: function(d) { return escapeHtml(d); } },
    { data: 'age' },
    { data: 'gender', render: function(d) { return escapeHtml(d); } },
    { data: 'studiesCount', render: function(d) { return '<span class="badge bg-secondary">' + d + '</span>'; } },
    {
      data: null, orderable: false, className: 'text-end',
      render: function(d) {
        return '<div class="d-flex justify-content-end gap-1">' +
          '<button class="btn btn-sm btn-outline-info" title="View studies" onclick="viewPatientStudies(' + d.patientId + ')">Studies</button>' +
          '<button class="btn btn-sm btn-outline-primary" onclick="editPatientClick(' + d.patientId + ')">Edit</button>' +
          '<button class="btn btn-sm btn-outline-danger" onclick="deletePatientClick(' + d.patientId + ')">Delete</button>' +
          '</div>';
      }
    }
  ],
  order: [[0, 'desc']],
  language: { emptyTable: 'No patients found.' },
  responsive: true
});

var doctorsTable = new DataTable('#doctorsTable', {
  ajax: { url: API.doctors, dataSrc: '' },
  columns: [
    { data: 'doctorId' },
    { data: null, render: function(d) { return escapeHtml(fullName(d.firstName, d.lastName)); } },
    { data: 'specialty', render: function(d) { return escapeHtml(d); } },
    { data: 'age' },
    { data: 'gender', render: function(d) { return escapeHtml(d); } },
    { data: 'phone', render: function(d) { return escapeHtml(d) || '-'; } },
    { data: 'email', render: function(d) { return escapeHtml(d) || '-'; } },
    { data: 'studiesCount', render: function(d) { return '<span class="badge bg-secondary">' + d + '</span>'; } },
    {
      data: null, orderable: false, className: 'text-end',
      render: function(d) {
        return '<div class="d-flex justify-content-end gap-1">' +
          '<button class="btn btn-sm btn-outline-primary" onclick="editDoctorClick(' + d.doctorId + ')">Edit</button>' +
          '<button class="btn btn-sm btn-outline-danger" onclick="deleteDoctorClick(' + d.doctorId + ')">Delete</button>' +
          '</div>';
      }
    }
  ],
  order: [[0, 'desc']],
  language: { emptyTable: 'No doctors found.' },
  responsive: true
});

var studiesTable = new DataTable('#studiesTable', {
  ajax: { url: API.studies, dataSrc: '' },
  columns: [
    { data: 'studyId' },
    { data: 'patientName', render: function(d) { return escapeHtml(d); } },
    { data: 'patientMrn', render: function(d) { return escapeHtml(d); } },
    { data: 'modality', render: function(d) { return escapeHtml(d); } },
    { data: 'studyDate', render: function(d) { return formatDate(d); } },
    { data: 'doctorName', render: function(d) { return escapeHtml(d); } },
    {
      data: null, orderable: false, className: 'text-end',
      render: function(d) {
        return '<div class="d-flex justify-content-end gap-1">' +
          '<button class="btn btn-sm btn-outline-primary" onclick="editStudyClick(' + d.studyId + ')">Edit</button>' +
          '<button class="btn btn-sm btn-outline-danger" onclick="deleteStudyClick(' + d.studyId + ')">Delete</button>' +
          '</div>';
      }
    }
  ],
  order: [[4, 'desc']],
  language: { emptyTable: 'No studies found.' },
  responsive: true
});

// ---------------- dropdown loaders ----------------

async function loadDoctorsDropdown(selectId, selectedId) {
  var doctors = await apiFetch(API.doctors, 'GET');
  var select = document.getElementById(selectId);
  select.innerHTML = '<option value="">Select doctor</option>' +
    doctors.map(function(d) {
      return '<option value="' + d.doctorId + '"' + (String(d.doctorId) === String(selectedId) ? ' selected' : '') + '>' +
        escapeHtml(fullName(d.firstName, d.lastName)) + ' (' + escapeHtml(d.specialty) + ')</option>';
    }).join('');
}

async function loadPatientsDropdown(selectId, selectedId) {
  var patients = await apiFetch(API.patients, 'GET');
  var select = document.getElementById(selectId);
  select.innerHTML = '<option value="">Select patient</option>' +
    patients.map(function(p) {
      return '<option value="' + p.patientId + '"' + (String(p.patientId) === String(selectedId) ? ' selected' : '') + '>' +
        escapeHtml(fullName(p.firstName, p.lastName)) + ' (' + escapeHtml(p.mrn) + ')</option>';
    }).join('');
}

// ============================================================
// PATIENT CRUD
// ============================================================

let currentPatientStatus = null;

function openPatientModal(patient) {
  var form = document.getElementById('patientForm');
  form.reset();
  form.classList.remove('was-validated');
  document.getElementById('patientModalTitle').textContent = patient ? 'Edit Patient' : 'Add Patient';
  document.getElementById('patientSubmitBtn').textContent = patient ? 'Update' : 'Save';

  if (patient) {
    document.getElementById('editPatientId').value = patient.patientId;
    document.getElementById('firstName').value = patient.firstName || '';
    document.getElementById('lastName').value = patient.lastName || '';
    document.getElementById('dateOfBirth').value = formatDate(patient.dateOfBirth);
    document.getElementById('gender').value = patient.gender || '';
    document.getElementById('phone').value = patient.phone || '';
    document.getElementById('email').value = patient.email || '';
    document.getElementById('mrn').value = patient.mrn || '';
    currentPatientStatus = patient.status;
  } else {
    document.getElementById('editPatientId').value = '';
    currentPatientStatus = 'Active';
  }

  bootstrap.Modal.getOrCreateInstance(document.getElementById('patientModal')).show();
}

function editPatientClick(id) {
  apiFetch(API.patients + '/' + id, 'GET')
    .then(function(p) { openPatientModal(p); })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

function deletePatientClick(id) {
  if (!confirm('Are you sure you want to delete this patient and all their studies?')) return;
  apiFetch(API.patients + '/' + id, 'DELETE')
    .then(function() {
      showToast('Patient deleted successfully.');
      patientsTable.ajax.reload();
      studiesTable.ajax.reload();
    })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

document.getElementById('patientForm').addEventListener('submit', async function(e) {
  e.preventDefault();
  var form = e.currentTarget;
  if (!form.checkValidity()) { form.classList.add('was-validated'); return; }

  var payload = {
    firstName: document.getElementById('firstName').value.trim(),
    lastName: document.getElementById('lastName').value.trim(),
    dateOfBirth: document.getElementById('dateOfBirth').value,
    gender: document.getElementById('gender').value,
    phone: document.getElementById('phone').value.trim() || null,
    email: document.getElementById('email').value.trim() || null,
    mrn: document.getElementById('mrn').value.trim(),
    status: currentPatientStatus
  };

  try {
    var id = document.getElementById('editPatientId').value;
    if (id) {
      await apiFetch(API.patients + '/' + id, 'PUT', payload);
      showToast('Patient updated successfully.');
    } else {
      await apiFetch(API.patients, 'POST', payload);
      showToast('Patient added successfully.');
    }
    bootstrap.Modal.getInstance(document.getElementById('patientModal')).hide();
    patientsTable.ajax.reload();
    studiesTable.ajax.reload();
  } catch (err) {
    showToast(err.message, 'danger');
  }
});

// ============================================================
// DOCTOR CRUD
// ============================================================

function openDoctorModal(doctor) {
  var form = document.getElementById('doctorForm');
  form.reset();
  form.classList.remove('was-validated');
  document.getElementById('doctorModalTitle').textContent = doctor ? 'Edit Doctor' : 'Add Doctor';
  document.getElementById('doctorSubmitBtn').textContent = doctor ? 'Update' : 'Save';

  if (doctor) {
    document.getElementById('editDoctorId').value = doctor.doctorId;
    document.getElementById('docFirstName').value = doctor.firstName || '';
    document.getElementById('docLastName').value = doctor.lastName || '';
    document.getElementById('docDob').value = formatDate(doctor.dateOfBirth);
    document.getElementById('docGender').value = doctor.gender || '';
    document.getElementById('docPhone').value = doctor.phone || '';
    document.getElementById('docEmail').value = doctor.email || '';
    document.getElementById('docSpecialty').value = doctor.specialty || '';
  } else {
    document.getElementById('editDoctorId').value = '';
  }

  bootstrap.Modal.getOrCreateInstance(document.getElementById('doctorModal')).show();
}

function editDoctorClick(id) {
  apiFetch(API.doctors + '/' + id, 'GET')
    .then(function(d) { openDoctorModal(d); })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

function deleteDoctorClick(id) {
  if (!confirm('Are you sure you want to delete this doctor?')) return;
  apiFetch(API.doctors + '/' + id, 'DELETE')
    .then(function() {
      showToast('Doctor deleted successfully.');
      doctorsTable.ajax.reload();
      studiesTable.ajax.reload();
    })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

document.getElementById('doctorForm').addEventListener('submit', async function(e) {
  e.preventDefault();
  var form = e.currentTarget;
  if (!form.checkValidity()) { form.classList.add('was-validated'); return; }

  var payload = {
    firstName: document.getElementById('docFirstName').value.trim(),
    lastName: document.getElementById('docLastName').value.trim(),
    dateOfBirth: document.getElementById('docDob').value,
    gender: document.getElementById('docGender').value,
    phone: document.getElementById('docPhone').value.trim() || null,
    email: document.getElementById('docEmail').value.trim() || null,
    specialty: document.getElementById('docSpecialty').value
  };

  try {
    var id = document.getElementById('editDoctorId').value;
    if (id) {
      await apiFetch(API.doctors + '/' + id, 'PUT', payload);
      showToast('Doctor updated successfully.');
    } else {
      await apiFetch(API.doctors, 'POST', payload);
      showToast('Doctor added successfully.');
    }
    bootstrap.Modal.getInstance(document.getElementById('doctorModal')).hide();
    doctorsTable.ajax.reload();
    studiesTable.ajax.reload();
  } catch (err) {
    showToast(err.message, 'danger');
  }
});

// ============================================================
// STUDY CRUD
// ============================================================

var currentStudyStatus = null;

async function openStudyModal(study) {
  var form = document.getElementById('studyForm');
  form.reset();
  form.classList.remove('was-validated');
  document.getElementById('studyModalTitle').textContent = study ? 'Edit Study' : 'Add Study';
  document.getElementById('studySubmitBtn').textContent = study ? 'Update' : 'Save';
  document.getElementById('editStudyId').value = study ? study.studyId : '';

  await loadPatientsDropdown('studyPatient', study ? study.patientId : null);
  await loadDoctorsDropdown('studyModalDoctor', study ? study.doctorId : null);

  if (study) {
    document.getElementById('studyModalModality').value = study.modality || '';
    document.getElementById('studyModalDate').value = formatDate(study.studyDate);
    currentStudyStatus = study.status;
  } else {
    document.getElementById('studyModalDate').value = todayStr();
    currentStudyStatus = 'Scheduled';
  }

  bootstrap.Modal.getOrCreateInstance(document.getElementById('studyModal')).show();
}

function editStudyClick(id) {
  apiFetch(API.studies + '/' + id, 'GET')
    .then(function(s) { openStudyModal(s); })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

function deleteStudyClick(id) {
  if (!confirm('Are you sure you want to delete this study?')) return;
  apiFetch(API.studies + '/' + id, 'DELETE')
    .then(function() {
      showToast('Study deleted successfully.');
      studiesTable.ajax.reload();
      patientsTable.ajax.reload();
    })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

document.getElementById('studyForm').addEventListener('submit', async function(e) {
  e.preventDefault();
  var form = e.currentTarget;
  if (!form.checkValidity()) { form.classList.add('was-validated'); return; }

  var payload = {
    patientId: parseInt(document.getElementById('studyPatient').value, 10),
    doctorId: parseInt(document.getElementById('studyModalDoctor').value, 10),
    modality: document.getElementById('studyModalModality').value,
    studyDate: document.getElementById('studyModalDate').value,
    status: currentStudyStatus
  };

  try {
    var id = document.getElementById('editStudyId').value;
    if (id) {
      await apiFetch(API.studies + '/' + id, 'PUT', payload);
      showToast('Study updated successfully.');
    } else {
      await apiFetch(API.studies, 'POST', payload);
      showToast('Study added successfully.');
    }
    bootstrap.Modal.getInstance(document.getElementById('studyModal')).hide();
    studiesTable.ajax.reload();
    patientsTable.ajax.reload();
  } catch (err) {
    showToast(err.message, 'danger');
  }
});

// ============================================================
// VIEW PATIENT STUDIES
// ============================================================

var currentViewPatientId = null;

function viewPatientStudies(patientId) {
  currentViewPatientId = patientId;
  var rows = patientsTable.rows().data().toArray();
  var p = rows.find(function(r) { return r.patientId === patientId; });
  document.getElementById('viewStudiesPatientName').textContent =
    p ? fullName(p.firstName, p.lastName) + ' (' + p.mrn + ')' : '#' + patientId;

  loadViewStudies(patientId);
  bootstrap.Modal.getOrCreateInstance(document.getElementById('viewStudiesModal')).show();
}

function loadViewStudies(patientId) {
  apiFetch(API.studies + '?patientId=' + patientId, 'GET')
    .then(function(studies) {
      var tbody = document.getElementById('viewStudiesTableBody');
      if (!studies.length) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted py-4">No studies for this patient.</td></tr>';
        return;
      }
      tbody.innerHTML = studies.map(function(s) {
        return '<tr>' +
          '<td>' + s.studyId + '</td>' +
          '<td>' + escapeHtml(s.modality) + '</td>' +
          '<td>' + formatDate(s.studyDate) + '</td>' +
          '<td>' + escapeHtml(s.doctorName) + '</td>' +
          '<td class="text-end">' +
            '<button class="btn btn-sm btn-outline-danger" onclick="deleteViewStudy(' + s.studyId + ')">Delete</button>' +
          '</td></tr>';
      }).join('');
    })
    .catch(function(err) { showToast(err.message, 'danger'); });
}

function deleteViewStudy(studyId) {
  if (!confirm('Are you sure you want to delete this study?')) return;
  apiFetch(API.studies + '/' + studyId, 'DELETE')
    .then(function() {
      showToast('Study deleted successfully.');
      loadViewStudies(currentViewPatientId);
      studiesTable.ajax.reload();
      patientsTable.ajax.reload();
    })
    .catch(function(err) { showToast(err.message, 'danger'); });
}
