(() => {
  const style = document.createElement('style');
  style.textContent = `
    .row-actions{display:flex;gap:6px;align-items:center;white-space:nowrap}
    .row-actions .btn{padding:7px 10px;font-size:11px}
    .filter-row{display:flex;align-items:center;gap:8px;flex-wrap:wrap}
    .filter-row input,.filter-row select{min-height:38px;border:1px solid #e0e6df;border-radius:9px;background:#fff;padding:8px 10px;color:#314239}
    .report-cards{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px;margin:18px 0}
    .report-stat{padding:17px;border:1px solid #e5ebe4;border-radius:13px;background:#fff}
    .report-stat span{display:block;color:#78847d;font-size:12px}.report-stat strong{display:block;margin-top:9px;font-size:24px;color:#244b3e;letter-spacing:-.04em}
    .report-bars{display:grid;grid-template-columns:repeat(7,minmax(35px,1fr));align-items:end;gap:10px;min-height:205px;padding:22px 15px 12px}
    .report-day{display:grid;grid-template-rows:1fr auto;gap:8px;min-width:0;height:165px;text-align:center;color:#849088;font-size:10px}
    .report-bar-wrap{display:flex;align-items:end;justify-content:center;height:135px;border-bottom:1px solid #e8ede7}
    .report-bar{width:min(28px,72%);min-height:3px;border-radius:6px 6px 2px 2px;background:linear-gradient(180deg,#6e9c7e,#2e6553)}
    .report-empty{grid-column:1/-1;align-self:center;text-align:center;color:#87918a;padding:38px}
    .history-list{display:grid;gap:9px;margin-top:15px}.history-item{display:grid;grid-template-columns:minmax(0,1fr) auto;gap:10px;padding:13px;border:1px solid #edf0ec;border-radius:10px}
    .history-item strong{display:block;font-size:13px}.history-item small{display:block;color:#7e8982;margin-top:4px}
    .empty-state{padding:34px 20px;text-align:center}.empty-state h3{margin:0 0 6px;font-size:17px;color:#304b3e}.empty-state p{max-width:420px;margin:0 auto 15px;color:#7a8780;font-size:13px;line-height:1.6}
    @media(max-width:650px){.table-wrap{overflow:visible}table{display:block;white-space:normal}table thead{display:none}table tbody{display:grid;gap:9px;padding:10px}table tbody tr{display:grid;border:1px solid #e9eee8;border-radius:11px;overflow:hidden;background:#fff}table tbody td{display:grid;grid-template-columns:minmax(78px,32%) minmax(0,1fr);gap:9px;align-items:start;padding:9px 11px;white-space:normal;overflow-wrap:anywhere}table tbody td:before{content:attr(data-label);color:#849088;font-size:10px;font-weight:700}table tbody td:last-child{border-bottom:0}table tbody td .row-actions{flex-wrap:wrap}table tbody td.empty{display:block;text-align:center}table tbody td.empty:before{content:none}}
    @media(max-width:800px){.report-cards{grid-template-columns:repeat(2,minmax(0,1fr))}}
    @media(max-width:560px){.row-actions{gap:4px}.row-actions .btn{padding:7px;font-size:10px}.filter-row>*{flex:1;min-width:130px}.report-cards{gap:8px}.report-stat{padding:13px}.report-stat strong{font-size:20px}.report-bars{gap:5px;padding-inline:8px}.report-day{font-size:9px}}
  `;
  document.head.append(style);

  titles.reports = 'Raporlar';

  window.render = async function () {
    const root = document.querySelector('#content');
    root.innerHTML = '<div class="empty">Yükleniyor…</div>';
    try {
      if (state.page === 'reports') await renderReports(root);
      else if (state.page === 'appointments') {
        if (state.appointmentsView === 'calendar') {
          await renderCalendar(root);
          root.querySelector('.welcome')?.insertAdjacentHTML('beforeend','<button class="btn soft" id="list-view">Liste görünümü</button>');
          root.querySelector('#list-view')?.addEventListener('click',()=>{state.appointmentsView='list';render()});
        } else await renderAppointments(root);
      }
      else if (['customers', 'staff', 'services', 'payments'].includes(state.page)) await renderList(root, state.page);
      else if (state.page === 'settings') await renderSettings(root);
      else await renderDashboard(root);
    } catch (error) {
      root.innerHTML = `<section class="panel"><div class="empty">${esc(error.message)}<br><button class="btn soft" style="margin-top:14px" onclick="render()">Yeniden dene</button></div></section>`;
    }
  };

  function safeDateInput(d) {
    const x = new Date(d);
    return `${x.getFullYear()}-${String(x.getMonth()+1).padStart(2,'0')}-${String(x.getDate()).padStart(2,'0')}`;
  }
  function safeDateTimeInput(d) {
    const x = new Date(d);
    return `${safeDateInput(x)}T${String(x.getHours()).padStart(2,'0')}:${String(x.getMinutes()).padStart(2,'0')}`;
  }
  function actionButton(action, id, label, kind) {
    return `<button type="button" class="btn soft" data-${action}="${kind}:${id}">${label}</button>`;
  }
  function actionCell(kind, id, detail = false) {
    return `<td><div class="row-actions">${detail ? actionButton('details', id, 'Geçmiş', kind) : ''}${actionButton('edit', id, 'Düzenle', kind)}</div></td>`;
  }
  function stampTableLabels(root, headers) {
    root.querySelectorAll('tbody tr').forEach(row=>row.querySelectorAll('td').forEach((cell,index)=>{
      if(!cell.classList.contains('empty'))cell.dataset.label=headers[index]||'';
    }));
  }
  function reminderActions(appointment) {
    const customer=appointment.customer||{}, digits=String(customer.phone||'').replace(/\D/g,'');
    const local=digits.startsWith('0')?digits.slice(1):digits;
    const phone=local.length===10&&local.startsWith('5')?`90${local}`:digits;
    const when=date(appointment.startTime), body=encodeURIComponent(`Merhaba ${customer.firstName||''}, ${when} tarihindeki ${appointment.service?.name||'randevun'} için hatırlatmak istedik. Görüşmek üzere, ${state.business}.`);
    const whatsapp=phone?`<a class="btn soft" target="_blank" rel="noopener" href="https://wa.me/${phone}?text=${body}">WhatsApp</a>`:'';
    const email=customer.email?`<a class="btn soft" href="mailto:${encodeURIComponent(customer.email)}?subject=${encodeURIComponent('Randevu hatırlatması')}&body=${body}">E-posta</a>`:'';
    return `${whatsapp}${email}`;
  }

  window.renderList = async function (root, page) {
    const search = page === 'customers' ? (document.querySelector('#search')?.value || '') : '';
    const canManage = ['Owner','Manager'].includes(state.role);
    const cfg = {
      customers: { endpoint:'/api/customers', head:['Müşteri','Telefon','E-posta','Kayıt','İşlem'], rows:items=>items.map(x=>`<tr><td><strong>${esc(x.firstName)} ${esc(x.lastName)}</strong></td><td>${esc(x.phone||'—')}</td><td>${esc(x.email||'—')}</td><td>${date(x.createdAt)}</td>${actionCell('customers',x.id,true)}</tr>`) },
      staff: { endpoint:'/api/staff', head:['Ekip üyesi','Görev','Telefon','E-posta','Durum',...(canManage?['İşlem']:[])], rows:items=>items.map(x=>`<tr><td><strong>${esc(x.firstName)} ${esc(x.lastName)}</strong></td><td>${esc(x.role)}</td><td>${esc(x.phone||'—')}</td><td>${esc(x.email||'—')}</td><td>${badge(x.isActive?'Aktif':'Pasif')}</td>${canManage?actionCell('staff',x.id):''}</tr>`) },
      services: { endpoint:'/api/services', head:['Hizmet','Süre','Ücret','Durum',...(canManage?['İşlem']:[])], rows:items=>items.map(x=>`<tr><td><strong>${esc(x.name)}</strong><div class="muted">${esc(x.description||'')}</div></td><td>${x.durationMinutes} dk</td><td>${money(x.price)}</td><td>${badge(x.isActive?'Aktif':'Pasif')}</td>${canManage?actionCell('services',x.id):''}</tr>`) },
      payments: { endpoint:'/api/payments', head:['Müşteri','Randevu','Tutar','Yöntem','Tarih','Durum',...(canManage?['İşlem']:[])], rows:items=>items.map(x=>`<tr><td>${esc(x.customer?.firstName)} ${esc(x.customer?.lastName)}</td><td>#${x.appointmentId}</td><td><strong>${money(x.amount)}</strong></td><td>${esc(statusText(x.paymentMethod))}</td><td>${date(x.createdAt)}</td><td>${badge(x.status)}</td>${canManage?actionCell('payments',x.id):''}</tr>`) }
    }[page];
    const query = new URLSearchParams({page:'1',pageSize:'100'});
    if (search) query.set('search', search);
    const data = await api(`${cfg.endpoint}?${query}`);
    const rows = cfg.rows(data.items || []);
    const createAllowed = !['staff','services','payments'].includes(page) || canManage;
    root.innerHTML = welcome(titles[page], ({customers:'Müşteri bilgilerini, notlarını ve ziyaret geçmişini tek yerde takip et.',staff:'Ekibinin iletişim bilgilerini ve durumunu güncel tut.',services:'Hizmetlerini, sürelerini ve fiyatlarını düzenle.',payments:'Tahsilat hareketlerini ve ödeme durumlarını takip et.'})[page], createAllowed ? `<button class="btn" data-create="${page}">＋ Yeni ${{customers:'müşteri',staff:'personel',services:'hizmet',payments:'ödeme'}[page]}</button>` : '')
      + `<section class="panel"><div class="panel-head"><h3>${data.totalItems??rows.length} kayıt</h3><div class="toolbar">${page==='customers'?`<input id="search" aria-label="Müşteri ara" placeholder="Ad, telefon veya e-posta ara" value="${esc(search)}">`:''}<button class="btn soft" id="refresh">Yenile</button></div></div>${rows.length?table(cfg.head,rows):`<div class="empty-state"><h3>${page==='customers'?'Müşteri listen boş':page==='staff'?'Ekibine ilk kişiyi ekle':page==='services'?'İlk hizmetini tanımla':'Henüz ödeme kaydı yok'}</h3><p>${page==='customers'?'Randevu ve iletişim bilgilerini tek yerde takip etmek için müşteri ekle.':page==='staff'?'Randevu planlamak için aktif ekip üyelerini ekle.':page==='services'?'Müşteriler çevrimiçi randevu alabilsin diye önce bir hizmet ekle.':'Bir randevu için ödeme kaydedildiğinde hareketler burada görünür.'}</p>${page==='payments'?'':`<button class="btn" data-create="${page}">＋ ${page==='customers'?'Müşteri':page==='staff'?'Personel':'Hizmet'} ekle</button>`}</div>`}</section>`;
    stampTableLabels(root,cfg.head);
    window.wireActions();
    document.querySelector('#refresh')?.addEventListener('click',()=>window.render());
    document.querySelector('#search')?.addEventListener('keydown',e=>{if(e.key==='Enter')window.render()});
    if (page==='customers' && data.totalItems>100) root.insertAdjacentHTML('beforeend','<p class="muted">İlk 100 sonuç gösteriliyor. Aramayı daraltarak diğer kayıtları bulabilirsin.</p>');
  };

  async function renderAppointments(root) {
    const today = new Date();
    const startValue = document.querySelector('#appointment-from')?.value || safeDateInput(today);
    const endValue = document.querySelector('#appointment-to')?.value || safeDateInput(new Date(today.getFullYear(),today.getMonth(),today.getDate()+30));
    const statusValue = document.querySelector('#appointment-status')?.value || '';
    const data = await api(`/api/appointments?startDate=${encodeURIComponent(`${startValue}T00:00:00`)}&endDate=${encodeURIComponent(`${endValue}T23:59:59`)}&page=1&pageSize=100`);
    let items = data.items || [];
    if (statusValue) items = items.filter(x=>x.status===statusValue);
    const rows = items.map(x=>`<tr><td><strong>${esc(x.customer?.firstName)} ${esc(x.customer?.lastName)}</strong><div class="muted">${esc(x.customer?.phone||'')}</div></td><td>${esc(x.service?.name)}</td><td>${esc(x.staff?.firstName)} ${esc(x.staff?.lastName)}</td><td>${date(x.startTime)}</td><td>${date(x.endTime)}</td><td>${badge(x.status)}</td><td><div class="row-actions">${actionButton('edit',x.id,'Düzenle','appointments')}${x.status==='Cancelled'?'':reminderActions(x)}</div></td></tr>`);
    root.innerHTML=welcome('Randevular','Randevuları tarihe göre bul; açıp bilgilerini ve durumunu düzenle.',`<div class="row-actions"><button class="btn soft" id="calendar-view">Takvim görünümü</button><button class="btn" data-create="appointments">＋ Randevu oluştur</button></div>`)
      + `<section class="panel"><div class="panel-head"><div><h3>${items.length} randevu</h3><span class="subtle">En fazla 100 sonuç gösterilir</span></div><div class="filter-row"><label class="muted">Başlangıç <input id="appointment-from" type="date" value="${startValue}"></label><label class="muted">Bitiş <input id="appointment-to" type="date" value="${endValue}"></label><select id="appointment-status" aria-label="Duruma göre filtrele"><option value="">Tüm durumlar</option>${[['Pending','Bekliyor'],['Confirmed','Onaylandı'],['Completed','Tamamlandı'],['Cancelled','İptal']].map(([v,l])=>`<option value="${v}" ${statusValue===v?'selected':''}>${l}</option>`).join('')}</select></div></div>${rows.length?table(['Müşteri','Hizmet','Personel','Başlangıç','Bitiş','Durum','İşlem'],rows):`<div class="empty-state"><h3>Bu aralıkta randevu yok</h3><p>Tarih aralığını değiştir veya yeni randevu ekle. Uygun saatler takvimde görünür.</p><button class="btn" data-create="appointments">＋ Randevu oluştur</button></div>`}</section>`;
    stampTableLabels(root,['Müşteri','Hizmet','Personel','Başlangıç','Bitiş','Durum','İşlem']);
    window.wireActions();
    document.getElementById('calendar-view').onclick=()=>{state.appointmentsView='calendar';render()};
    for(const id of ['appointment-from','appointment-to','appointment-status']) document.getElementById(id).onchange=()=>window.render();
    if(data.totalItems>100) root.insertAdjacentHTML('beforeend','<p class="muted">Bu aralıkta 100’den fazla randevu var. Daha kısa tarih aralığı seç.</p>');
  }

  async function renderReports(root) {
    const now=new Date(), monthStart=new Date(now.getFullYear(),now.getMonth(),1);
    const from=document.querySelector('#report-from')?.value||safeDateInput(monthStart);
    const to=document.querySelector('#report-to')?.value||safeDateInput(now);
    const report=await api(`/api/dashboard/reports?from=${from}&to=${to}`);
    const max=Math.max(1,...report.daily.map(x=>Number(x.revenue)||0));
    const lastDays=report.daily.slice(-14), step=Math.ceil(lastDays.length/7);
    const bars=Array.from({length:Math.min(7,lastDays.length)},(_,i)=>{
      const group=lastDays.slice(i*step,(i+1)*step), total=group.reduce((n,x)=>n+Number(x.revenue||0),0), ap=group.reduce((n,x)=>n+x.appointments,0), label=group.length?new Intl.DateTimeFormat('tr-TR',{day:'numeric',month:'short'}).format(new Date(group[0].date)):'';
      return `<div class="report-day" title="${money(total)} · ${ap} randevu"><div class="report-bar-wrap"><div class="report-bar" style="height:${Math.max(3,total/max*100)}%"></div></div><span>${label}</span></div>`;
    }).join('');
    const top=report.topServices.length?report.topServices.map(x=>`<div class="summary-item"><span>${esc(x.name)}</span><strong>${x.count} randevu</strong></div>`).join(''):'<div class="empty">Bu tarih aralığında hizmet kaydı yok.</div>';
    root.innerHTML=welcome('Raporlar','Randevu yoğunluğunu ve kaydedilen tahsilatları tarih aralığına göre incele.',`<button class="btn soft" id="export-report">CSV indir</button>`)
      + `<div class="filter-row"><label class="muted">Başlangıç <input id="report-from" type="date" value="${from}"></label><label class="muted">Bitiş <input id="report-to" type="date" value="${to}"></label></div>
      <div class="report-cards"><div class="report-stat"><span>Toplam randevu</span><strong>${report.appointmentCount}</strong></div><div class="report-stat"><span>Tamamlanan</span><strong>${report.completedCount}</strong></div><div class="report-stat"><span>Tahsil edilen</span><strong>${money(report.revenue)}</strong></div><div class="report-stat"><span>Bekleyen ödeme</span><strong>${money(report.pendingPayments)}</strong></div></div>
      <div class="dashboard-grid"><section class="panel"><div class="panel-head"><div><h3>Tahsilat görünümü</h3><span class="subtle">Son 14 gün, haftalık gruplar</span></div></div><div class="report-bars">${bars||'<div class="report-empty">Bu tarih aralığında veri yok.</div>'}</div></section><section class="panel"><div class="panel-head"><div><h3>En çok randevu alan hizmetler</h3><span class="subtle">Seçili tarih aralığı</span></div></div><div class="summary-list">${top}</div></section></div>`;
    for(const id of ['report-from','report-to']) document.getElementById(id).onchange=()=>window.render();
    document.getElementById('export-report').onclick=()=>{
      const csv=[['Tarih','Randevu','Tahsilat'],...report.daily.map(x=>[x.date,x.appointments,x.revenue])].map(row=>row.map(value=>`"${String(value).replaceAll('"','""')}"`).join(';')).join('\r\n');
      const a=document.createElement('a');a.href=URL.createObjectURL(new Blob(['\ufeff'+csv],{type:'text/csv;charset=utf-8'}));a.download=`ritma-rapor-${from}-${to}.csv`;a.click();URL.revokeObjectURL(a.href);
    };
  }

  window.openEdit = async function (kind,id) {
    const entity=await api(`/api/${kind}/${id}`);
    const root=document.querySelector('#modal-root');
    let title='',fields='';
    const select=(label,name,items,value,format)=>`<div class="field"><label>${label}</label><select name="${name}" required>${items.map(x=>`<option value="${x.id}" ${String(x.id)===String(value)?'selected':''}>${esc(format(x))}</option>`).join('')}</select></div>`;
    if(kind==='customers') {title='Müşteri bilgilerini düzenle';fields=field('Ad','FirstName','text',true,`value="${esc(entity.firstName)}"`)+field('Soyad','LastName','text',true,`value="${esc(entity.lastName)}"`)+field('Telefon','Phone','tel',true,`value="${esc(entity.phone||'')}"`)+field('E-posta','Email','email',false,`value="${esc(entity.email||'')}"`)+`<div class="field span2"><label>Not</label><textarea name="Notes">${esc(entity.notes||'')}</textarea></div>`;}
    if(kind==='staff') {title='Ekip üyesini düzenle';fields=field('Ad','FirstName','text',true,`value="${esc(entity.firstName)}"`)+field('Soyad','LastName','text',true,`value="${esc(entity.lastName)}"`)+field('Görev','Role','text',true,`value="${esc(entity.role)}"`)+field('Telefon','Phone','tel',false,`value="${esc(entity.phone||'')}"`)+field('E-posta','Email','email',false,`value="${esc(entity.email||'')}"`)+`<div class="field"><label>Durum</label><select name="IsActive"><option value="true" ${entity.isActive?'selected':''}>Aktif</option><option value="false" ${!entity.isActive?'selected':''}>Pasif</option></select></div>`;}
    if(kind==='services') {title='Hizmeti düzenle';fields=field('Hizmet adı','Name','text',true,`value="${esc(entity.name)}"`)+field('Süre (dakika)','DurationMinutes','number',true,`min="1" max="1440" value="${entity.durationMinutes}"`)+field('Ücret (₺)','Price','number',true,`min="0" step="0.01" value="${entity.price}"`)+`<div class="field span2"><label>Açıklama</label><textarea name="Description">${esc(entity.description||'')}</textarea></div><div class="field"><label>Durum</label><select name="IsActive"><option value="true" ${entity.isActive?'selected':''}>Aktif</option><option value="false" ${!entity.isActive?'selected':''}>Pasif</option></select></div>`;}
    if(kind==='payments') {title='Ödeme durumunu düzenle';fields=`<div class="field"><label>Durum</label><select name="Status">${[['Pending','Bekliyor'],['Paid','Ödendi'],['Refunded','İade edildi'],['Cancelled','İptal']].map(([v,l])=>`<option value="${v}" ${entity.status===v?'selected':''}>${l}</option>`).join('')}</select></div><div class="field span2"><label>Not</label><textarea name="Notes">${esc(entity.notes||'')}</textarea></div><p class="muted span2">Tutar ${money(entity.amount)} olarak kaydedildi. Finansal kayıt tutarlılığı için sonradan değiştirilemez.</p>`;}
    if(kind==='appointments') {
      const [customers,staff,services]=await Promise.all([api('/api/customers?pageSize=100'),api('/api/staff?includeInactive=true&pageSize=100'),api('/api/services?includeInactive=true&pageSize=100')]);
      title='Randevuyu düzenle';fields=select('Müşteri','CustomerId',customers.items,entity.customerId,x=>`${x.firstName} ${x.lastName}`)+select('Personel','StaffId',staff.items.filter(x=>x.isActive||x.id===entity.staffId),entity.staffId,x=>`${x.firstName} ${x.lastName}`)+select('Hizmet','ServiceId',services.items.filter(x=>x.isActive||x.id===entity.serviceId),entity.serviceId,x=>x.name)+field('Başlangıç','StartTime','datetime-local',true,`value="${safeDateTimeInput(entity.startTime)}"`)+field('Bitiş','EndTime','datetime-local',true,`value="${safeDateTimeInput(entity.endTime)}"`)+`<div class="field"><label>Durum</label><select name="Status">${[['Pending','Bekliyor'],['Confirmed','Onaylandı'],['Completed','Tamamlandı'],['Cancelled','İptal']].map(([v,l])=>`<option value="${v}" ${entity.status===v?'selected':''}>${l}</option>`).join('')}</select></div><div class="field span2"><label>Not</label><textarea name="Notes">${esc(entity.notes||'')}</textarea></div>`;
    }
    root.innerHTML=`<div class="modal-backdrop"><form id="edit-form" class="modal"><div class="modal-head"><h2>${title}</h2><button type="button" class="close" aria-label="Kapat">×</button></div><div class="form-grid">${fields}</div><div id="edit-error" class="error"></div><div class="modal-actions"><button type="button" class="btn soft close-action">Vazgeç</button><button class="btn">Değişiklikleri kaydet</button></div></form></div>`;
    root.querySelectorAll('.close,.close-action').forEach(b=>b.onclick=()=>root.innerHTML='');root.firstElementChild.onclick=e=>{if(e.target===root.firstElementChild)root.innerHTML=''};
    root.querySelector('#edit-form').onsubmit=async e=>{
      e.preventDefault();const body=Object.fromEntries(new FormData(e.currentTarget).entries());
      for(const key of ['CustomerId','StaffId','ServiceId','DurationMinutes'])if(body[key])body[key]=Number(body[key]);
      if(body.Price!==undefined)body.Price=Number(body.Price);
      if(body.IsActive!==undefined)body.IsActive=body.IsActive==='true';
      for(const key of ['Phone','Email','Notes','Description'])if(body[key]==='')body[key]=null;
      try{await api(`/api/${kind}/${id}`,{method:'PUT',body:JSON.stringify(body)});root.innerHTML='';toast('Değişiklikler kaydedildi.');window.render();}
      catch(error){document.querySelector('#edit-error').textContent=error.message;}
    };
  };

  window.showCustomerHistory = async function(id) {
    const history=await api(`/api/customers/${id}/history`), c=history.customer;
    const root=document.querySelector('#modal-root');
    const records=history.appointments.map(a=>`<div class="history-item"><div><strong>${esc(a.service)} · ${date(a.startTime)}</strong><small>${esc(a.staff)} · ${statusText(a.status)}</small></div><div>${a.payment?money(a.payment.amount):'—'}</div></div>`).join('')||'<div class="empty">Henüz randevu geçmişi yok.</div>';
    root.innerHTML=`<div class="modal-backdrop"><section class="modal"><div class="modal-head"><h2>${esc(c.firstName)} ${esc(c.lastName)}</h2><button class="close" aria-label="Kapat">×</button></div><p class="muted">${esc(c.phone||'')} · ${esc(c.email||'')}</p>${c.notes?`<div class="panel-head"><span>Müşteri notu</span></div><p>${esc(c.notes)}</p>`:''}<h3>Ziyaret geçmişi</h3><div class="history-list">${records}</div><div class="modal-actions"><button class="btn soft close-action">Kapat</button><button class="btn" data-edit="customers:${id}">Müşteriyi düzenle</button></div></section></div>`;
    root.querySelectorAll('.close,.close-action').forEach(b=>b.onclick=()=>root.innerHTML='');root.querySelector('[data-edit]').onclick=()=>window.openEdit('customers',id);
  };

  window.wireActions = function() {
    document.querySelectorAll('#content [data-page]').forEach(b=>b.onclick=()=>setPage(b.dataset.page));
    document.querySelectorAll('#content [data-create]').forEach(b=>b.onclick=()=>openCreate(b.dataset.create));
    document.querySelectorAll('#content [data-edit]').forEach(b=>b.onclick=()=>{const [kind,id]=b.dataset.edit.split(':');window.openEdit(kind,id).catch(e=>toast(e.message));});
    document.querySelectorAll('#content [data-details]').forEach(b=>b.onclick=()=>window.showCustomerHistory(b.dataset.details.split(':')[1]).catch(e=>toast(e.message)));
  };

  const originalOpenCreate = openCreate;
  window.openCreate = async function(kind) {
    await originalOpenCreate(kind);
    if (kind !== 'appointments') return;
    const form=document.querySelector('#create-form');
    if(!form) return;
    const updateEnd=async()=>{
      const serviceId=Number(form.elements.ServiceId.value),startValue=form.elements.StartTime.value;
      if(!serviceId||!startValue)return;
      try{const service=await api(`/api/services/${serviceId}`),end=new Date(startValue);end.setMinutes(end.getMinutes()+service.durationMinutes);form.elements.EndTime.value=safeDateTimeInput(end)}catch{}
    };
    form.elements.ServiceId.addEventListener('change',updateEnd);
    form.elements.StartTime.addEventListener('change',updateEnd);
  };

  if (state.token) showApp(); else showAuth();
})();
