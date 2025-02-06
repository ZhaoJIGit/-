﻿using Notes.APP.Common;
using Notes.APP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes.APP.Services
{
    public class NoteService
    {
        private DBHelper dBHelper;
        public NoteService()
        {
            dBHelper = new DBHelper();
        }
        public bool AddNote(NoteModel model)
        {
            var sql = $@" INSERT INTO NoteInfo (NoteId,NoteName,
                                Content,
                                CreateTime,
                                UpdateTime,
                                Color,
                                BackgroundColor,
                                PageBackgroundColor,
                                Opacity, 
                                XAxis,
                                YAxis,
                                Height,
                                Width,
                                Fixed,
                                IsDeleted) 
                          Values(@NoteId,@NoteName,
                                @Content,
                                @CreateTime,
                                @UpdateTime,
                                @Color,
                                @BackgroundColor,
                                @PageBackgroundColor,
                                @Opacity, 
                                @XAxis,
                                @YAxis,
                                @Height,
                                @Width,
                                @Fixed,
                                @IsDeleted)";
            var result = dBHelper.ExecuteNonQuery(sql, model);
            return result > 0;
        }
        public bool UpdateNote(NoteModel model)
        {
            var sql = $@" Update NoteInfo set Fixed=@Fixed,Height=@Height,Width=@Width, Content =@Content,UpdateTime=@UpdateTime ,BackgroundColor=@BackgroundColor ,Color =@Color,Opacity=@Opacity,XAxis=@XAxis,YAxis=@YAxis where NoteId =@NoteId";
            var result = dBHelper.ExecuteNonQuery(sql, model);
            return result > 0;
        }
        public bool SaveNote(NoteModel model)
        {
            var note = GetNote(model.NoteId);
            if (note != null)
            {
               return UpdateNote(model);
            }
            else
            {
               return AddNote(model);
            }
        }

        public NoteModel GetNote(string id)
        {
            var sql = $@" SELECT * FROM NoteInfo WHERE NoteId = '{id}'";
            var result = dBHelper.ExecuteReaderToModel<NoteModel>(sql);
            return result;
        }
        public List<NoteModel> GetNotes()
        {
            var sql = $@" SELECT * FROM NoteInfo WHERE IsDeleted = 0";
            var result = dBHelper.ExecuteReader<NoteModel>(sql);
            return result;
        }
    }
}
