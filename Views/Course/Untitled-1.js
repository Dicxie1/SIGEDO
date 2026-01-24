
        const btDeletes = document.querySelectorAll('.btn-delete');
        btnDelete.forEach(e =>{
            e.addEventListener('click', function(event){
                Swal.fire({
                    toast: true,
                    showCancelButton: true,
                    html: `Quieres eliminar <strong> ${e.dataset.student} ${e.dataset.name}</strong> de curso <strong> ${e.dataset.code}  ${"@Model!.CourseName.ToString()"}</strong> `
                    }).then((response) =>{
                        let studentId = e.dataset.student;
                        let idCourse = e.dataset.code;
                        let formData = new FormData();
                        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                        const token = tokenInput ? tokenInput.value : '';
                        formData.append('StudentId', studentId);
                        formData.append('IdCourse', idCourse);
                        formData.append('__RequestVerificationToken', token);
                        if(response.isConfirmed){
                            fetch(`/Course/Delete/${studentId}`,{
                                method: 'POST',
                                body: formData
                        }).then( r=>{ return r.json()}).
                            then( response =>{
                                if(response.success){
                                    Swal.fire({
                                        title: 'Accion Completada',
                                        text: `Se elimino correctamente ${e.dataset.name}`
                                    })
                                    e.closest('tr').remove();
                                }else{
                                    Swal.fire({
                                        title: 'Erro',
                                        text: `error: ${response.message}`})
                                }
                            })
                            
                        }
                    });
            });
        })