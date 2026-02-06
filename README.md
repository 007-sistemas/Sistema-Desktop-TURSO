# 🎯 Sistema de Produção Biométrico

## ✅ Executar

```
C:\Users\seu usuario\Downloads\BiometricSystem\bin\publish\BiometricSystem.exe
```

## 📋 Funcionalidades

- ✅ Cadastro de cooperados com biometria (DigitalPersona 4500 U.are)
- ✅ Registro de produção (Entrada/Saída)
- ✅ Banco SQLite local (`biometric.db`)
- ✅ Sincronização com Turso (libSQL/SQLite Cloud)
- ✅ **Novo:** Cadastro de Biometria com sincronização de cooperados do Turso

## 📊 Banco de Dados

### SQLite Local
Localizado em: `bin/publish/biometric.db`

**Tabelas:**
- `Employees` - Cooperados cadastrados
- `TimeRecords` - Registros de produção

### Turso (libSQL/SQLite Cloud)
Conexão configurada para sincronizar com Turso

**Tabelas principais:**
- `cooperados` - Lista de profissionais cadastrados
- `biometrias` - Armazenamento de digitais capturadas
- `pontos` - Registros de produção sincronizados

## 🆕 Nova Funcionalidade: Cadastrar Biometria

### Como usar:
1. Clique no botão **"👆 Cadastrar Biometria"** na tela principal
2. A lista de cooperados será carregada automaticamente do Turso
3. Selecione o cooperado na lista suspensa
4. Posicione o dedo no leitor biométrico
5. Clique em **"☝️ Capturar Digital"**
6. Salve a biometria clicando em **"💾 Salvar Biometria"**

### Arquivos implementados:
- `Database/TursoCooperadoHelper.cs` - Consulta cooperados do Turso
- `Forms/CadastrarBiometriaForm.cs` - Interface de cadastro
- `Forms/LoginForm.Designer.cs` - Botão integrado na tela principal

### Documentação completa:
Veja [GUIA_CADASTRAR_BIOMETRIA.md](GUIA_CADASTRAR_BIOMETRIA.md)

## 🔧 Desenvolvido em

- C# .NET 8.0
- Windows Forms
- SQLite (local)
- Turso (libSQL/SQLite Cloud)
- LibSQLClient (driver libSQL)
- DigitalPersona SDK
